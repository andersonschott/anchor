namespace Aschott.Anchor.AspNetCore.Endpoints;

/// <summary>
/// Marks an <see cref="IEndpoint"/> with its API version. Consumed by
/// versioning conventions added on top of the framework (out of scope for F1).
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class EndpointVersionAttribute(int version) : Attribute
{
    public int Version { get; } = version;
}
