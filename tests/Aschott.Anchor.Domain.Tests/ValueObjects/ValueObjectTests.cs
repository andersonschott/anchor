using Aschott.Anchor.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace Aschott.Anchor.Domain.Tests.ValueObjects;

public sealed class ValueObjectTests
{
    private sealed class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    [Fact]
    public void VOs_with_same_components_are_equal()
    {
        var a = new Money(10m, "BRL");
        var b = new Money(10m, "BRL");

        a.Equals(b).ShouldBeTrue();
        (a == b).ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void VOs_with_different_components_are_not_equal()
    {
        var a = new Money(10m, "BRL");
        var b = new Money(10m, "USD");
        var c = new Money(20m, "BRL");

        a.Equals(b).ShouldBeFalse();
        a.Equals(c).ShouldBeFalse();
        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void VO_compared_to_null_returns_false()
    {
        var a = new Money(10m, "BRL");

        a.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void VOs_handle_null_components()
    {
        var a = new Money(10m, null!);
        var b = new Money(10m, null!);

        a.Equals(b).ShouldBeTrue();
    }
}
