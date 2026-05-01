namespace Aschott.Anchor.Domain.Auditing;

/// <summary>
/// Marks an entity for which CreatedAt/UpdatedAt and CreatedBy/UpdatedBy
/// are managed automatically by infrastructure (EF audit conventions).
/// </summary>
public interface IAuditedObject
{
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
    string? CreatedBy { get; }
    string? UpdatedBy { get; }
}
