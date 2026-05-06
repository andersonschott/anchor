namespace Aschott.Anchor.Application.MultiTenancy;

/// <summary>
/// Marks a request type as requiring a non-null tenant context.
/// Enforced at the Mediator pipeline by <c>TenantContextBehavior</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class RequiresTenantAttribute : Attribute;
