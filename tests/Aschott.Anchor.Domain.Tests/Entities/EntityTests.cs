using Aschott.Anchor.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Aschott.Anchor.Domain.Tests.Entities;

public sealed class EntityTests
{
    private sealed class SampleEntity(Guid id) : Entity<Guid>(id);
    private sealed class OtherEntity(Guid id) : Entity<Guid>(id);

    [Fact]
    public void Equals_returns_true_for_same_type_and_same_id()
    {
        var id = Guid.NewGuid();
        var a = new SampleEntity(id);
        var b = new SampleEntity(id);

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equals_returns_false_for_same_type_different_ids()
    {
        var a = new SampleEntity(Guid.NewGuid());
        var b = new SampleEntity(Guid.NewGuid());

        a.Equals(b).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Equals_returns_false_for_different_concrete_types_even_with_same_id()
    {
        var id = Guid.NewGuid();
        var a = new SampleEntity(id);
        var b = new OtherEntity(id);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_returns_false_when_compared_to_null()
    {
        var a = new SampleEntity(Guid.NewGuid());

        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Default_id_is_default_TKey()
    {
        var entity = new SampleEntityNoArg();

        entity.Id.Should().Be(Guid.Empty);
    }

    private sealed class SampleEntityNoArg : Entity<Guid> { }
}
