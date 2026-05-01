using Aschott.Anchor.Infrastructure.MultiTenancy;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Infrastructure.Tests.Persistence;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design", "CA1001:Types that own disposable fields should be disposable",
    Justification = "Disposal handled via IAsyncLifetime.DisposeAsync, called by xUnit.")]
public sealed class TestDatabaseFixture : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    public CurrentTenantAccessor CurrentTenant { get; } = new();
    public DbContextOptions<TestDbContext> Options { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();
        Options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .Options;

        await using var ctx = new TestDbContext(Options, CurrentTenant);
        await ctx.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    public TestDbContext NewContext() => new(Options, CurrentTenant);
}

public sealed class BaseDbContextTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact]
    public async Task Query_filter_isolates_entities_by_tenant()
    {
        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();

        await SeedAsync(t1, "Alice");
        await SeedAsync(t2, "Bob");

        using (fixture.CurrentTenant.Change(t1))
        {
            await using var ctx = fixture.NewContext();
            var visible = await ctx.Customers.Where(c => c.Name == "Alice" || c.Name == "Bob").ToListAsync();

            visible.Count.ShouldBe(1);
            visible[0].Name.ShouldBe("Alice");
        }
    }

    [Fact]
    public async Task Null_tenant_bypass_returns_all_rows_for_seeded_tenants()
    {
        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();

        await SeedAsync(t1, "Carol");
        await SeedAsync(t2, "Dave");

        await using var ctx = fixture.NewContext();
        var visible = await ctx.Customers
            .Where(c => c.Name == "Carol" || c.Name == "Dave")
            .OrderBy(c => c.Name)
            .ToListAsync();

        visible.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Save_changes_stamps_TenantId_on_added_aggregate_when_omitted()
    {
        var tenant = Guid.NewGuid();

        using (fixture.CurrentTenant.Change(tenant))
        {
            await using var ctx = fixture.NewContext();
            var customer = new TestCustomer(Guid.NewGuid(), tenantId: null, "Eve");
            await ctx.Customers.AddAsync(customer);
            await ctx.SaveChangesAsync();

            customer.TenantId.ShouldBe(tenant);
        }
    }

    [Fact]
    public async Task Save_changes_populates_CreatedAt_and_UpdatedAt_for_audited_entities()
    {
        var tenant = Guid.NewGuid();
        var before = DateTime.UtcNow.AddSeconds(-1);
        var id = Guid.NewGuid();

        using (fixture.CurrentTenant.Change(tenant))
        {
            await using (var ctx = fixture.NewContext())
            {
                await ctx.Customers.AddAsync(new TestCustomer(id, tenant, "Frank"));
                await ctx.SaveChangesAsync();
            }

            await using (var ctx = fixture.NewContext())
            {
                var customer = await ctx.Customers.SingleAsync(c => c.Id == id);
                customer.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
                customer.UpdatedAt.ShouldBeNull();

                customer.Rename("Frances");
                await ctx.SaveChangesAsync();
            }

            await using (var ctx = fixture.NewContext())
            {
                var customer = await ctx.Customers.SingleAsync(c => c.Id == id);
                customer.UpdatedAt.ShouldNotBeNull();
                customer.UpdatedAt!.Value.ShouldBeGreaterThanOrEqualTo(customer.CreatedAt);
            }
        }
    }

    private async Task SeedAsync(Guid tenantId, string name)
    {
        using (fixture.CurrentTenant.Change(tenantId))
        {
            await using var ctx = fixture.NewContext();
            await ctx.Customers.AddAsync(new TestCustomer(Guid.NewGuid(), tenantId, name));
            await ctx.SaveChangesAsync();
        }
    }
}
