namespace Aschott.Anchor.Domain.Entities;

public interface IMultiTenant
{
    Guid? TenantId { get; }
}
