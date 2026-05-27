namespace Aschott.Anchor.Infrastructure.Persistence;

/// <summary>
/// Marker contract for a <see cref="Microsoft.EntityFrameworkCore.DbContext"/>
/// that exposes the current tenant id as an instance property so EF Core's
/// global query filter can reference it as a per-query parameter.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Aschott.Anchor.Infrastructure.Persistence.Conventions.MultiTenantQueryFilters.ApplyMultiTenantFilters"/>
/// requires the owning <see cref="Microsoft.EntityFrameworkCore.DbContext"/> to
/// expose this property so EF's funcletizer can build a
/// <see cref="Microsoft.EntityFrameworkCore.Query.QueryParameterExpression"/>
/// rooted on the live <see cref="Microsoft.EntityFrameworkCore.DbContext"/>
/// instance (re-evaluated per query). A plain
/// <see cref="Aschott.Anchor.Application.MultiTenancy.ICurrentTenant"/> would
/// be constant-folded at translation time.
/// </para>
/// <para>
/// <see cref="Aschott.Anchor.Infrastructure.Persistence.BaseDbContext"/>
/// implements this directly. Consumers that cannot extend
/// <see cref="BaseDbContext"/> (e.g. they extend
/// <c>IdentityDbContext&lt;TUser, TRole, TKey&gt;</c>) implement this interface
/// themselves and forward the value from their ambient tenant accessor.
/// </para>
/// </remarks>
public interface ITenantAwareDbContext
{
    /// <summary>
    /// The tenant id used by the global query filter for the next query
    /// execution on this context. A <see langword="null"/> value triggers the
    /// host/admin bypass (returns every row regardless of tenant).
    /// </summary>
    Guid? CurrentTenantId { get; }
}
