using NUnit.Framework;
using PricingCalc.Model.Engine.Core;
using PricingCalc.Model.Tests.Infrastructure.Model.Entities;

namespace PricingCalc.Model.Tests
{
    public class EntityPropertiesTests
    {
        [Test]
        public void CopyPropertiesTest()
        {
            var initial = (IFirstEntityProperties)new FirstEntityProperties()
            {
                NullableStringProperty = "abc"
            };
            var copy = initial.Copy();

            Assert.That(ReferenceEquals(initial, copy), Is.False);
            Assert.That(initial.Equals(copy), Is.True);
            Assert.That(initial == copy, Is.False);
            Assert.That(initial.NullableStringProperty, Is.EqualTo(copy.NullableStringProperty));
        }

        [Test]
        public void PropertyReadFromTest()
        {
            var props = (IFirstEntityProperties)new FirstEntityProperties();
            var bag = new PropertiesBag();
            var value = "abc";

            bag.Write(nameof(IFirstEntityProperties.NonNullableStringProperty), value);
            bag.Write<string>(nameof(IFirstEntityProperties.NullableStringProperty), null);
            bag.Write<string>(nameof(IFirstEntityProperties.NullableStringWithDefaultValueProperty), null);

            props.ReadFrom(bag);

            Assert.That(props.NonNullableStringProperty, Is.EqualTo(value));
            Assert.That(props.NullableStringProperty, Is.Null);
            Assert.That(props.NullableStringWithDefaultValueProperty, Is.Null);
        }

        [Test]
        public void PropertyWriteToTest()
        {
            var value = "abc";
            var props = (IFirstEntityProperties)new FirstEntityProperties()
            {
                NonNullableStringProperty = value
            };
            var bag = new PropertiesBag();

            props.WriteTo(bag);

            Assert.That(bag.Read<string>(nameof(IFirstEntityProperties.NonNullableStringProperty)), Is.EqualTo(value));
            Assert.That(bag.Read<string>(nameof(IFirstEntityProperties.NullableStringProperty)), Is.Null);
            Assert.That(bag.Read<string>(nameof(IFirstEntityProperties.NullableStringWithDefaultValueProperty)), Is.Null);
        }

        [Test]
        public void EqualsTest()
        {
            var value = "abc";
            var first = (IFirstEntityProperties)new FirstEntityProperties()
            {
                NonNullableStringProperty = value
            };
            var second = (IFirstEntityProperties)new FirstEntityProperties()
            {
                NonNullableStringProperty = value
            };
            var third = (IFirstEntityProperties)new FirstEntityProperties()
            {
                NonNullableStringProperty = "different"
            };

            Assert.That(ReferenceEquals(first, second), Is.False);
            Assert.That(ReferenceEquals(first, third), Is.False);
            Assert.That(first.Equals(second), Is.True);
            Assert.That(first.Equals(third), Is.False);
        }
    }
}
