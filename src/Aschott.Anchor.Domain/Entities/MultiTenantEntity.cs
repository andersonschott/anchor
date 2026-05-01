namespace Aschott.Anchor.Domain.Entities;

public abstract class MultiTenantEntity<TKey> : AggregateRoot<TKey>, IMultiTenant
{
    public Guid? TenantId { get; private set; }

    protected MultiTenantEntity() { }
    protected MultiTenantEntity(TKey id, Guid? tenantId) : base(id) => TenantId = tenantId;

    internal void SetTenantId(Guid? tenantId) => TenantId = tenantId;
}
