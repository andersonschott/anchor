namespace Aschott.Anchor.Application.MultiTenancy;

public interface ICurrentTenant
{
    Guid? Id { get; }

    /// <summary>Temporarily change the current tenant. Returns IDisposable that reverts on dispose.</summary>
    IDisposable Change(Guid? tenantId);
}
