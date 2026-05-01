namespace Aschott.Anchor.Infrastructure.Persistence;

/// <summary>
/// Application-facing surface of the underlying DbContext, restricted to the
/// commit operation. Allows command handlers to depend on this abstraction
/// instead of EF's full DbContext API.
/// </summary>
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
