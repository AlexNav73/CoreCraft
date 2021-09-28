using System;
using NUnit.Framework;
using PricingCalc.Model.Tests.Infrastructure.Model.Entities;

namespace PricingCalc.Model.Tests
{
    public class EntitiesTests
    {
        [Test]
        public void CopyEntityTest()
        {
            var id = Guid.NewGuid();
            var initial = new FirstEntity(id);
            var copy = initial with { };

            Assert.That(ReferenceEquals(initial, copy), Is.False);
            Assert.That(initial.Equals(copy), Is.True);
            Assert.That(initial == copy, Is.True);
            Assert.That(initial.Id, Is.EqualTo(copy.Id));
        }
    }
}
