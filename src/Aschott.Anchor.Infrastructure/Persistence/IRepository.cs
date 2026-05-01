using Aschott.Anchor.Domain.Entities;

namespace Aschott.Anchor.Infrastructure.Persistence;

public interface IRepository<TEntity, TKey> where TEntity : Entity<TKey>
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}
