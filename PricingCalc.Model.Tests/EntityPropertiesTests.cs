using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Tests;

public class EntityPropertiesTests
{
    [Test]
    public void CopyPropertiesTest()
    {
        var initial = new FirstEntityProperties()
        {
            NullableStringProperty = "abc"
        };
        var copy = initial with { };

        Assert.That(ReferenceEquals(initial, copy), Is.False);
        Assert.That(initial.Equals(copy), Is.True);
        Assert.That(initial == copy, Is.True);
        Assert.That(initial.NullableStringProperty, Is.EqualTo(copy.NullableStringProperty));
    }

    [Test]
    public void PropertyReadFromTest()
    {
        var props = new FirstEntityProperties();
        var bag = new PropertiesBag();
        var value = "abc";

        bag.Write(nameof(FirstEntityProperties.NonNullableStringProperty), value);
        bag.Write<string>(nameof(FirstEntityProperties.NullableStringProperty), null);
        bag.Write<string>(nameof(FirstEntityProperties.NullableStringWithDefaultValueProperty), null);

        props = (FirstEntityProperties)props.ReadFrom(bag);

        Assert.That(props.NonNullableStringProperty, Is.EqualTo(value));
        Assert.That(props.NullableStringProperty, Is.Null);
        Assert.That(props.NullableStringWithDefaultValueProperty, Is.Null);
    }

    [Test]
    public void PropertyWriteToTest()
    {
        var value = "abc";
        var props = new FirstEntityProperties()
        {
            NonNullableStringProperty = value
        };
        var bag = new PropertiesBag();

        props.WriteTo(bag);

        Assert.That(bag.Read<string>(nameof(FirstEntityProperties.NonNullableStringProperty)), Is.EqualTo(value));
        Assert.That(bag.Read<string>(nameof(FirstEntityProperties.NullableStringProperty)), Is.Null);
        Assert.That(bag.Read<string>(nameof(FirstEntityProperties.NullableStringWithDefaultValueProperty)), Is.Null);
    }

    [Test]
    public void EqualsTest()
    {
        var value = "abc";
        var first = new FirstEntityProperties()
        {
            NonNullableStringProperty = value
        };
        var second = new FirstEntityProperties()
        {
            NonNullableStringProperty = value
        };
        var third = new FirstEntityProperties()
        {
            NonNullableStringProperty = "different"
        };

        Assert.That(ReferenceEquals(first, second), Is.False);
        Assert.That(ReferenceEquals(first, third), Is.False);
        Assert.That(first.Equals(second), Is.True);
        Assert.That(first.Equals(third), Is.False);
    }
}
