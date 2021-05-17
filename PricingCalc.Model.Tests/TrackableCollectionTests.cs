using System;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using PricingCalc.Model.Engine.ChangesTracking;
using PricingCalc.Model.Engine.Core;
using PricingCalc.Model.Tests.Model.Entities;

namespace PricingCalc.Model.Tests
{
    public class TrackableCollectionTests
    {
        private ICollection<IFirstEntity, IFirstEntityProperties> _collection;
        private ICollectionChanges<IFirstEntity, IFirstEntityProperties> _changes;
        private ICollection<IFirstEntity, IFirstEntityProperties> _trackable;

        [SetUp]
        public void Setup()
        {
            _collection = A.Fake<ICollection<IFirstEntity, IFirstEntityProperties>>();
            var factory = A.Fake<IFactory<IFirstEntity, IFirstEntityProperties>>();
            A.CallTo(() => factory.EntityFactory).Returns(id => new FirstEntity(id));
            A.CallTo(() => factory.DataFactory).Returns(() => new FirstEntityProperties());
            A.CallTo(() => _collection.Create()).Returns(new EntityBuilder<IFirstEntity, IFirstEntityProperties>((e, d) => {}, factory));

            _changes = new CollectionChanges<IFirstEntity, IFirstEntityProperties>();
            _trackable = new TrackableCollection<IFirstEntity, IFirstEntityProperties>(_changes, _collection);
        }

        [Test]
        public void TrackableAddToCollectionTest()
        {
            var entity = _trackable.Create().Build();
            var change = _changes.Single();

            A.CallTo(() => _collection.Create()).MustHaveHappened();
            Assert.That(_changes.HasChanges(), Is.True);
            Assert.That(change.Action, Is.EqualTo(EntityAction.Add));
            Assert.That(change.Entity, Is.Not.Null);
            Assert.That(change.NewData, Is.Not.Null);
            Assert.That(change.OldData, Is.Null);
        }

        [Test]
        public void TrackableRemoveFromCollectionTest()
        {
            _trackable.Remove(A.Dummy<IFirstEntity>());

            var change = _changes.Single();

            A.CallTo(() => _collection.Remove(A<IFirstEntity>.Ignored)).MustHaveHappened();
            Assert.That(_changes.HasChanges(), Is.True);
            Assert.That(change.Action, Is.EqualTo(EntityAction.Remove));
            Assert.That(change.Entity, Is.Not.Null);
            Assert.That(change.NewData, Is.Null);
            Assert.That(change.OldData, Is.Not.Null);
        }

        [Test]
        public void TrackableModifyEntityInsideCollectionTest()
        {
            _trackable.Modify(A.Dummy<IFirstEntity>(), p => p.NullableStringProperty = "test");

            var change = _changes.Single();

            A.CallTo(() => _collection.Modify(A<IFirstEntity>.Ignored, A<Action<IFirstEntityProperties>>.Ignored))
                .MustHaveHappened();
            Assert.That(_changes.HasChanges(), Is.True);
            Assert.That(change.Action, Is.EqualTo(EntityAction.Modify));
            Assert.That(change.Entity, Is.Not.Null);
            Assert.That(change.NewData, Is.Not.Null);
            Assert.That(change.OldData, Is.Not.Null);
        }
    }
}
