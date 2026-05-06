using Aschott.Anchor.Application.MultiTenancy;

namespace Aschott.Anchor.Infrastructure.MultiTenancy;

/// <summary>
/// AsyncLocal-based <see cref="ICurrentTenant"/> implementation suitable for
/// CLI tools, background workers, and tests. AspNetCore consumers swap in an
/// HttpContext-backed implementation.
/// </summary>
public sealed class CurrentTenantAccessor : ICurrentTenant
{
    private readonly AsyncLocal<Guid?> _override = new();

    public Guid? Id => _override.Value;

    public IDisposable Change(Guid? tenantId)
    {
        var previous = _override.Value;
        _override.Value = tenantId;
        return new Reverter(() => _override.Value = previous);
    }

    private sealed class Reverter(Action onDispose) : IDisposable
    {
        public void Dispose() => onDispose();
    }
}
