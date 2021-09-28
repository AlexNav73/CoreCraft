using System;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using PricingCalc.Model.Engine.ChangesTracking;
using PricingCalc.Model.Engine.Core;
using PricingCalc.Model.Tests.Infrastructure.Model.Entities;

namespace PricingCalc.Model.Tests
{
    public class TrackableCollectionTests
    {
        private ICollection<FirstEntity, FirstEntityProperties> _collection;
        private ICollectionChangeSet<FirstEntity, FirstEntityProperties> _changes;
        private ICollection<FirstEntity, FirstEntityProperties> _trackable;

        [SetUp]
        public void Setup()
        {
            _collection = A.Fake<ICollection<FirstEntity, FirstEntityProperties>>();

            _changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
            _trackable = new TrackableCollection<FirstEntity, FirstEntityProperties>(_changes, _collection);
        }

        [Test]
        public void TrackableAddToCollectionTest()
        {
            _trackable.Add(new());
            var change = _changes.Single();

            A.CallTo(() => _collection.Add(A<FirstEntityProperties>.Ignored)).MustHaveHappened();
            Assert.That(_changes.HasChanges(), Is.True);
            Assert.That(change.Action, Is.EqualTo(CollectionAction.Add));
            Assert.That(change.Entity, Is.Not.Null);
            Assert.That(change.NewData, Is.Not.Null);
            Assert.That(change.OldData, Is.Null);
        }

        [Test]
        public void TrackableRemoveFromCollectionTest()
        {
            A.CallTo(() => _collection.Get(A<FirstEntity>.Ignored)).Returns(new FirstEntityProperties());

            _trackable.Remove(A.Dummy<FirstEntity>());

            var change = _changes.Single();

            A.CallTo(() => _collection.Remove(A<FirstEntity>.Ignored)).MustHaveHappened();
            Assert.That(_changes.HasChanges(), Is.True);
            Assert.That(change.Action, Is.EqualTo(CollectionAction.Remove));
            Assert.That(change.Entity, Is.Not.Null);
            Assert.That(change.NewData, Is.Null);
            Assert.That(change.OldData, Is.Not.Null);
        }

        [Test]
        public void TrackableModifyEntityInsideCollectionTest()
        {
            A.CallTo(() => _collection.Get(A<FirstEntity>.Ignored))
                .ReturnsNextFromSequence(
                    new FirstEntityProperties(),
                    new FirstEntityProperties() { NullableStringProperty = "test" }
                );

            _trackable.Modify(A.Dummy<FirstEntity>(), p => p with { NullableStringProperty = "test" });

            var change = _changes.Single();

            A.CallTo(() => _collection.Modify(A<FirstEntity>.Ignored, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
                .MustHaveHappened();
            Assert.That(_changes.HasChanges(), Is.True);
            Assert.That(change.Action, Is.EqualTo(CollectionAction.Modify));
            Assert.That(change.Entity, Is.Not.Null);
            Assert.That(change.NewData, Is.Not.Null);
            Assert.That(change.OldData, Is.Not.Null);
        }
    }
}
