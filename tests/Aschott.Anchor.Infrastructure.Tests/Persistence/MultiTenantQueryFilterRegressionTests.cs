using System.Globalization;
using Aschott.Anchor.Infrastructure.MultiTenancy;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Infrastructure.Tests.Persistence;

/// <summary>
/// Regression coverage for the multi-tenant query-filter literal bug.
///
/// <para>
/// History: prior to the <c>fix/tenant-filter-literal</c> change, the filter
/// closed directly over <c>currentTenant.Id</c>. EF Core's funcletizer
/// evaluated the property at translation time, embedded the FIRST tenant's id
/// as a SQL literal, and cached the plan. Every subsequent request — regardless
/// of the actual tenant — was filtered by the first request's tenant id,
/// causing cross-tenant data hiding and potential leakage in long-running
/// processes.
/// </para>
///
/// <para>
/// These tests share a <see cref="DbContextOptions"/> instance across queries
/// so the EF model is built once and the compiled plan is cached — exactly the
/// production code path. Any reappearance of the literal-folding bug fails
/// <see cref="Switching_current_tenant_returns_each_tenants_rows_in_the_same_process"/>.
/// </para>
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design", "CA1001:Types that own disposable fields should be disposable",
    Justification = "Disposal handled via IAsyncLifetime.DisposeAsync, called by xUnit.")]
public sealed class MultiTenantQueryFilterRegressionFixture : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private ILoggerFactory _loggerFactory = null!;

    public CurrentTenantAccessor CurrentTenant { get; } = new();

    public DbContextOptions<TestDbContext> Options { get; private set; } = null!;

    public CapturingLoggerProvider LoggerProvider { get; } = new();

    public Guid TenantA { get; } = Guid.NewGuid();

    public Guid TenantB { get; } = Guid.NewGuid();

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        _loggerFactory = LoggerFactory.Create(b => b.AddProvider(LoggerProvider).SetMinimumLevel(LogLevel.Information));

        Options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .UseLoggerFactory(_loggerFactory)
            .EnableSensitiveDataLogging()
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.AmbientTransactionWarning))
            .Options;

        await using var ctx = new TestDbContext(Options, CurrentTenant);
        await ctx.Database.EnsureCreatedAsync();

        // Seed two rows per tenant under each tenant's identity so the
        // SaveChanges tenant stamping path is exercised.
        await SeedAsync(TenantA, "tenantA-row-1");
        await SeedAsync(TenantA, "tenantA-row-2");
        await SeedAsync(TenantB, "tenantB-row-1");
        await SeedAsync(TenantB, "tenantB-row-2");
    }

    public async Task DisposeAsync()
    {
        _loggerFactory.Dispose();
        await _connection.DisposeAsync();
    }

    public TestDbContext NewContext() => new(Options, CurrentTenant);

    private async Task SeedAsync(Guid tenantId, string name)
    {
        using (CurrentTenant.Change(tenantId))
        {
            await using var ctx = NewContext();
            await ctx.Customers.AddAsync(new TestCustomer(Guid.NewGuid(), tenantId, name));
            await ctx.SaveChangesAsync();
        }
    }
}

public sealed class MultiTenantQueryFilterRegressionTests(MultiTenantQueryFilterRegressionFixture fixture)
    : IClassFixture<MultiTenantQueryFilterRegressionFixture>
{
    /// <summary>
    /// Critical regression. Reproduces the literal-folding scenario: build the
    /// model and warm the plan with tenant A, then switch to tenant B in the
    /// SAME process / SAME options instance. If the tenant id were baked into
    /// the SQL as a literal, B's query would still be filtered by A's id and
    /// return zero rows.
    /// </summary>
    [Fact]
    public async Task Switching_current_tenant_returns_each_tenants_rows_in_the_same_process()
    {
        // Warm: first query under tenant A. This is where the funcletizer
        // historically produced a constant-folded SQL plan.
        using (fixture.CurrentTenant.Change(fixture.TenantA))
        {
            await using var ctx = fixture.NewContext();
            var rowsA = await ctx.Customers.OrderBy(c => c.Name).ToListAsync();

            rowsA.Count.ShouldBe(2);
            rowsA.ShouldAllBe(c => c.TenantId == fixture.TenantA);
        }

        // Switch tenant in the same process. With the bug, this used to return
        // rowsA (the cached literal) or empty results depending on the cached
        // plan's tenant id.
        using (fixture.CurrentTenant.Change(fixture.TenantB))
        {
            await using var ctx = fixture.NewContext();
            var rowsB = await ctx.Customers.OrderBy(c => c.Name).ToListAsync();

            rowsB.Count.ShouldBe(2);
            rowsB.ShouldAllBe(c => c.TenantId == fixture.TenantB);
            rowsB.ShouldNotContain(c => c.TenantId == fixture.TenantA);
        }

        // Switch back to tenant A — proves the plan is genuinely
        // re-parameterized, not just frozen on the most recent value.
        using (fixture.CurrentTenant.Change(fixture.TenantA))
        {
            await using var ctx = fixture.NewContext();
            var rowsA2 = await ctx.Customers.OrderBy(c => c.Name).ToListAsync();

            rowsA2.Count.ShouldBe(2);
            rowsA2.ShouldAllBe(c => c.TenantId == fixture.TenantA);
        }
    }

    /// <summary>
    /// Verifies that the host-mode escape hatch survives the parameterization
    /// change: with <c>currentTenant.Id == null</c> every row passes the filter.
    /// </summary>
    [Fact]
    public async Task Null_current_tenant_bypasses_filter_and_returns_all_rows()
    {
        // No Change scope → ambient AsyncLocal id is null → host/admin mode.
        await using var ctx = fixture.NewContext();
        fixture.CurrentTenant.Id.ShouldBeNull();

        var all = await ctx.Customers.OrderBy(c => c.Name).ToListAsync();

        all.Count.ShouldBe(4);
        all.ShouldContain(c => c.TenantId == fixture.TenantA);
        all.ShouldContain(c => c.TenantId == fixture.TenantB);
    }

    /// <summary>
    /// Inspects the SQL EF emits for a filtered query. The fix is correct iff
    /// the tenant id is sent via a parameter binding — the raw Guid must never
    /// appear inline in the SQL command body (the part after the
    /// <c>Parameters=[...]</c> log prefix), and the SELECT must reference a
    /// parameter placeholder in its WHERE clause.
    /// </summary>
    [Fact]
    public async Task Generated_sql_uses_parameter_not_literal_for_tenant_id()
    {
        fixture.LoggerProvider.Clear();

        using (fixture.CurrentTenant.Change(fixture.TenantA))
        {
            await using var ctx = fixture.NewContext();
            _ = await ctx.Customers.ToListAsync();
        }

        var raw = fixture.LoggerProvider.LastSqlOrEmpty();
        raw.ShouldNotBeNullOrWhiteSpace("EF Core should have logged the SELECT statement");

        // EF logs commands as:
        //   Executed DbCommand (0ms) [Parameters=[@p='value', ...], ...]\n<SQL>
        // We must inspect only the SQL body (after the closing ']' of the
        // metadata block) — the Parameters list legitimately echoes the
        // parameter value when sensitive data logging is enabled, and that's
        // exactly the correct, parameterized path.
        var sqlOnly = ExtractSqlBody(raw);
        sqlOnly.ShouldNotBeNullOrWhiteSpace("Failed to isolate the SQL command body from the EF log entry");

        // Defensive: the SELECT must reference the Customers table.
        sqlOnly.ShouldContain("Customers");

        // A literal-folded plan would inline the Guid here ('e678b50f-...').
        var tenantALiteral = fixture.TenantA.ToString("D", CultureInfo.InvariantCulture);
        sqlOnly.ShouldNotContain(tenantALiteral,
            customMessage: "Tenant id leaked into the SQL body as a literal. EF is constant-folding the filter again.");

        // Positive assertion: WHERE clause must reference a parameter (@<name>).
        sqlOnly.ShouldContain("@",
            customMessage: "Expected EF to emit at least one parameter placeholder in the SQL body.");
    }

    private static string ExtractSqlBody(string commandLog)
    {
        // The log entry shape is:
        //   "Executed DbCommand (...) [Parameters=[...], CommandType='Text', CommandTimeout='30']\nSELECT ..."
        // Find the closing ']' of the bracketed metadata block, then take what follows.
        var newline = commandLog.IndexOf('\n', StringComparison.Ordinal);
        return newline < 0 ? commandLog : commandLog[(newline + 1)..];
    }
}

public sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly List<string> _messages = [];
    private readonly Lock _gate = new();

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(this);

    public void Dispose() { }

    public void Clear()
    {
        lock (_gate) _messages.Clear();
    }

    public string LastSqlOrEmpty()
    {
        lock (_gate)
        {
            // Find the most recent message that looks like a SELECT statement.
            for (var i = _messages.Count - 1; i >= 0; i--)
            {
                if (_messages[i].Contains("SELECT", StringComparison.Ordinal))
                    return _messages[i];
            }
            return string.Empty;
        }
    }

    internal void Add(string message)
    {
        lock (_gate) _messages.Add(message);
    }

    private sealed class CapturingLogger(CapturingLoggerProvider owner) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            owner.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
