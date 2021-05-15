using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PricingCalc.Model.Engine.Core;
using PricingCalc.Model.Tests.Model.Entities;

namespace PricingCalc.Model.Tests
{
    public class CollectionTests
    {
        private ICollection<IFirstEntity, IFirstEntityProperties> _collection;

        [SetUp]
        public void Setup()
        {
            _collection = new Collection<IFirstEntity, IFirstEntityProperties>(id => new FirstEntity(id), () => new FirstEntityProperties());
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
            _collection.Add(new FirstEntity(firstEntityId), new FirstEntityProperties());

            Assert.That(_collection.Count, Is.EqualTo(1));
            Assert.That(_collection.Single().Id, Is.EqualTo(firstEntityId));
        }

        [Test]
        public void CreateEntityWithFluidApiTest()
        {
            Assert.That(_collection.Count, Is.EqualTo(0));

            var firstEntityId = Guid.NewGuid();
            var entity = _collection.Create()
                .WithId(firstEntityId)
                .WithInit(p => { })
                .Build();

            Assert.That(_collection.Count, Is.EqualTo(1));
            Assert.That(_collection.Single(), Is.EqualTo(entity));
            Assert.That(entity.Id, Is.EqualTo(firstEntityId));
        }

        [Test]
        public void RemoveEntitiyFromCollectionTest()
        {
            var entity = _collection.Create()
                .WithInit(p => { })
                .Build();

            Assert.That(_collection.Count, Is.EqualTo(1));

            _collection.Remove(entity);

            Assert.That(_collection.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetPropertiesByEntityTest()
        {
            var value = "test";
            var entity = _collection.Create()
                .WithInit(p =>
                {
                    p.NonNullableStringProperty = value;
                })
                .Build();

            Assert.That(_collection.Count, Is.EqualTo(1));

            var properties = _collection.Get(entity);

            Assert.That(properties, Is.Not.Null);
            Assert.That(properties.NonNullableStringProperty, Is.EqualTo(value));
        }

        [Test]
        public void GetWithInvalidEntityTest()
        {
            var entity = _collection.Create().Build();

            Assert.That(_collection.Count, Is.EqualTo(1));

            var properties = _collection.Get(entity);

            Assert.That(properties, Is.Not.Null);
            Assert.Throws<KeyNotFoundException>(() => _collection.Get(new FirstEntity(Guid.NewGuid())));
        }

        [Test]
        public void GetFromEmptyCollectionTest()
        {
            Assert.That(_collection.Count, Is.EqualTo(0));
            Assert.Throws<KeyNotFoundException>(() => _collection.Get(new FirstEntity(Guid.NewGuid())));
        }

        [Test]
        public void ModifyPropertiesTest()
        {
            var value = "test";
            var newValue = "new value";

            var entity = _collection.Create()
                .WithInit(p =>
                {
                    p.NonNullableStringProperty = value;
                })
                .Build();

            var properties = _collection.Get(entity);
            Assert.That(properties.NonNullableStringProperty, Is.EqualTo(value));

            _collection.Modify(entity, p => p.NonNullableStringProperty = newValue);
            properties = _collection.Get(entity);

            Assert.That(properties, Is.Not.Null);
            Assert.That(properties.NonNullableStringProperty, Is.EqualTo(newValue));
        }

        [Test]
        public void ModifyNotExistingEntityTest()
        {
            Assert.That(_collection.Count, Is.EqualTo(0));
            Assert.Throws<KeyNotFoundException>(() =>
                _collection.Modify(new FirstEntity(Guid.NewGuid()), p => p.NullableStringProperty = "test"));
        }

        [Test]
        public void CopyCollection()
        {
            var value = "test";
            var entity = _collection.Create()
                .WithInit(p => p.NonNullableStringProperty = value)
                .Build();

            var copy = _collection.Copy();
            var copiedEntity = _collection.Single();

            Assert.That(ReferenceEquals(_collection, copy), Is.False);
            Assert.That(_collection.Count, Is.EqualTo(copy.Count));
            Assert.That(entity, Is.EqualTo(copiedEntity));
        }
    }
}
