namespace Aschott.Anchor.Domain.Entities;

public abstract class Entity<TKey> : IEquatable<Entity<TKey>>
{
    public TKey Id { get; protected set; } = default!;

    protected Entity() { }
    protected Entity(TKey id) => Id = id;

    public bool Equals(Entity<TKey>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return EqualityComparer<TKey>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj) => obj is Entity<TKey> e && Equals(e);

    public override int GetHashCode()
        => HashCode.Combine(GetType(), EqualityComparer<TKey>.Default.GetHashCode(Id!));

    public static bool operator ==(Entity<TKey>? left, Entity<TKey>? right)
        => Equals(left, right);

    public static bool operator !=(Entity<TKey>? left, Entity<TKey>? right)
        => !Equals(left, right);
}
