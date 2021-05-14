using System;
using System.Linq;
using NUnit.Framework;
using PricingCalc.Model.Engine.Core;
using PricingCalc.Model.Engine.Fluent;
using PricingCalc.Model.Tests.Model.Entities;

namespace PricingCalc.Model.Tests
{
    public class CollectionTests
    {
        private ICollection<IFirstEntity, IFirstEntityProperties> _collection;

        [SetUp]
        public void Setup()
        {
            _collection = new Collection<IFirstEntity, IFirstEntityProperties>(new EntityFactory<FirstEntity, FirstEntityProperties>(id => new FirstEntity() { Id = id }));
        }

        [TearDown]
        public void Teardown()
        {
            _collection = null;
        }

        [Test]
        public void AddEntityToCollectionTest()
        {
            Assert.That(_collection.Count, Is.EqualTo(0));

            var firstEntityId = Guid.NewGuid();
            _collection.Add(new FirstEntity() { Id = firstEntityId }, new FirstEntityProperties());

            Assert.That(_collection.Count, Is.EqualTo(1));
            Assert.That(_collection.First().Id, Is.EqualTo(firstEntityId));
        }

        [Test]
        public void CreateEntityWithFluidApiTest()
        {
            Assert.That(_collection.Count, Is.EqualTo(0));

            var firstEntityId = Guid.NewGuid();
            var entity = _collection.Create(firstEntityId)
                .Initialize(p => { })
                .Finish();

            Assert.That(_collection.Count, Is.EqualTo(1));
            Assert.That(_collection.First(), Is.EqualTo(entity));
            Assert.That(entity.Id, Is.EqualTo(firstEntityId));
        }

        [Test]
        public void RemoveEntitiyFromCollectionTest()
        {
            var entity = _collection.Create()
                .Initialize(p => { })
                .Finish();

            Assert.That(_collection.Count, Is.EqualTo(1));

            _collection.Remove(entity);

            Assert.That(_collection.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetPropertiesByEntityTest()
        {
            var value = "test";
            var entity = _collection.Create()
                .Initialize(p =>
                {
                    p.NonNullableStringProperty = value;
                })
                .Finish();

            Assert.That(_collection.Count, Is.EqualTo(1));

            var properties = _collection.Get(entity);

            Assert.That(properties, Is.Not.Null);
            Assert.That(properties.NonNullableStringProperty, Is.EqualTo(value));
        }

        [Test]
        public void ModifyPropertiesTest()
        {
            var value = "test";
            var newValue = "new value";

            var entity = _collection.Create()
                .Initialize(p =>
                {
                    p.NonNullableStringProperty = value;
                })
                .Finish();

            var properties = _collection.Get(entity);
            Assert.That(properties.NonNullableStringProperty, Is.EqualTo(value));

            _collection.Modify(entity, p => p.NonNullableStringProperty = newValue);
            properties = _collection.Get(entity);

            Assert.That(properties, Is.Not.Null);
            Assert.That(properties.NonNullableStringProperty, Is.EqualTo(newValue));
        }
    }
}
