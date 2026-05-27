using System.Linq.Expressions;
using System.Reflection;
using Aschott.Anchor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aschott.Anchor.Infrastructure.Persistence.Conventions;

/// <summary>
/// Registers an EF Core global query filter on every <see cref="IMultiTenant"/>
/// entity so that queries return only rows belonging to the current tenant.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this is not a one-liner.</b> The naive form
/// <c>e =&gt; e.TenantId == currentTenant.Id || currentTenant.Id == null</c>
/// is unsafe across requests in long-running processes (web hosts, workers).
/// EF Core's funcletizer evaluates <c>currentTenant.Id</c> — a property access
/// on a captured singleton reference — at translation time, embeds the FIRST
/// tenant's id as a SQL literal (no <c>@p0</c>), and caches the plan. Every
/// subsequent request is then filtered by the first request's tenant id,
/// causing cross-tenant hiding and potential leakage.
/// </para>
/// <para>
/// <b>The fix.</b> EF's
/// <c>Microsoft.EntityFrameworkCore.Query.Internal.ExpressionTreeFuncletizer</c>
/// has a special path for member access rooted on the
/// <see cref="DbContext"/> instance: it produces a
/// <see cref="Microsoft.EntityFrameworkCore.Query.QueryParameterExpression"/>
/// re-evaluated per query (the value is read off the <i>actual</i> context
/// executing the query, not a snapshot taken at model-build time). We therefore
/// build the filter as
/// <c>e =&gt; e.TenantId == ctx.CurrentTenantId || ctx.CurrentTenantId == null</c>
/// where <c>ctx</c> is the <see cref="ITenantAwareDbContext"/> being configured.
/// This is the same mechanism documented for
/// <see href="https://learn.microsoft.com/ef/core/querying/filters#query-filters-and-ientitytypeconfiguration">IEntityTypeConfiguration with a tenant id</see>
/// and for
/// <see href="https://learn.microsoft.com/ef/core/querying/filters#using-context-data---multi-tenancy">constructor-captured tenantId on the context</see>.
/// </para>
/// <para>
/// The relational pipeline translates the parameter expression to a
/// per-execution <c>SqlParameterExpression</c>. Confirm in logs: SQL contains
/// <c>WHERE [t].[TenantId] = @__ef_filter__p_0</c> with
/// <c>Parameters=[@__ef_filter__p_0='...']</c>.
/// </para>
/// </remarks>
public static class MultiTenantQueryFilters
{
    private static readonly MethodInfo BuildFilterMethod =
        typeof(MultiTenantQueryFilters).GetMethod(
            nameof(BuildFilter),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly PropertyInfo CurrentTenantIdProperty =
        typeof(ITenantAwareDbContext).GetProperty(
            nameof(ITenantAwareDbContext.CurrentTenantId),
            BindingFlags.Instance | BindingFlags.Public)!;

    /// <summary>
    /// Applies a per-query, parameterized tenant filter to every
    /// <see cref="IMultiTenant"/> entity in the model:
    /// <c>e.TenantId == context.CurrentTenantId || context.CurrentTenantId == null</c>.
    /// The null branch preserves the host/admin bypass (system queries with
    /// <c>CurrentTenantId == null</c> see every row).
    /// </summary>
    /// <param name="modelBuilder">EF Core model builder.</param>
    /// <param name="context">
    /// The owning <see cref="DbContext"/>. Pass <c>this</c> from
    /// <c>OnModelCreating</c>. The context must implement
    /// <see cref="ITenantAwareDbContext"/>; EF reads
    /// <see cref="ITenantAwareDbContext.CurrentTenantId"/> fresh for each query
    /// execution. <see cref="BaseDbContext"/> implements the interface
    /// automatically; consumers extending <c>IdentityDbContext</c> (or other
    /// non-<see cref="BaseDbContext"/> bases) implement it themselves.
    /// </param>
    public static void ApplyMultiTenantFilters(this ModelBuilder modelBuilder, ITenantAwareDbContext context)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(e => typeof(IMultiTenant).IsAssignableFrom(e.ClrType)))
        {
            var lambda = (LambdaExpression)BuildFilterMethod
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(null, [context])!;

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    private static Expression<Func<TEntity, bool>> BuildFilter<TEntity>(ITenantAwareDbContext context)
        where TEntity : class, IMultiTenant
    {
        // Build:  e => e.TenantId == context.CurrentTenantId
        //           || context.CurrentTenantId == null
        // The MemberExpression on a DbContext-typed Constant is what EF's
        // funcletizer reliably translates to a per-query parameter (the
        // expression tree retains the property access; the funcletizer
        // re-reads the value at every query execution rather than freezing
        // it at model-build time).
        var entity = Expression.Parameter(typeof(TEntity), "e");
        var contextConstant = Expression.Constant(context, typeof(ITenantAwareDbContext));
        var contextTenantId = Expression.Property(contextConstant, CurrentTenantIdProperty);
        var entityTenantId = Expression.Property(entity, nameof(IMultiTenant.TenantId));

        var tenantMatches = Expression.Equal(entityTenantId, contextTenantId);
        var nullableType = typeof(Guid?);
        var nullConstant = Expression.Constant(null, nullableType);
        var hostBypass = Expression.Equal(contextTenantId, nullConstant);

        var body = Expression.OrElse(tenantMatches, hostBypass);
        return Expression.Lambda<Func<TEntity, bool>>(body, entity);
    }
}
