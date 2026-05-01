using Aschott.Anchor.Domain.Auditing;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Domain.Tests.Auditing;

public sealed class IAuditedObjectTests
{
    [Fact]
    public void Exposes_four_audit_properties()
    {
        var t = typeof(IAuditedObject);

        t.GetProperty(nameof(IAuditedObject.CreatedAt))!.PropertyType.ShouldBe(typeof(DateTime));
        t.GetProperty(nameof(IAuditedObject.UpdatedAt))!.PropertyType.ShouldBe(typeof(DateTime?));
        t.GetProperty(nameof(IAuditedObject.CreatedBy))!.PropertyType.ShouldBe(typeof(string));
        t.GetProperty(nameof(IAuditedObject.UpdatedBy))!.PropertyType.ShouldBe(typeof(string));
    }
}
