using Aschott.Anchor.Domain.Entities;
using Shouldly;
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

        a.Equals(b).ShouldBeTrue();
        (a == b).ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void Equals_returns_false_for_same_type_different_ids()
    {
        var a = new SampleEntity(Guid.NewGuid());
        var b = new SampleEntity(Guid.NewGuid());

        a.Equals(b).ShouldBeFalse();
        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void Equals_returns_false_for_different_concrete_types_even_with_same_id()
    {
        var id = Guid.NewGuid();
        var a = new SampleEntity(id);
        var b = new OtherEntity(id);

        a.Equals(b).ShouldBeFalse();
    }

    [Fact]
    public void Equals_returns_false_when_compared_to_null()
    {
        var a = new SampleEntity(Guid.NewGuid());

        a.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void Default_id_is_default_TKey()
    {
        var entity = new SampleEntityNoArg();

        entity.Id.ShouldBe(Guid.Empty);
    }

    private sealed class SampleEntityNoArg : Entity<Guid> { }
}
