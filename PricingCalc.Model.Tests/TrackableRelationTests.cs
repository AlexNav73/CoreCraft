using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using PricingCalc.Model.Engine.ChangesTracking;
using PricingCalc.Model.Engine.Core;
using PricingCalc.Model.Tests.Model.Entities;

namespace PricingCalc.Model.Tests
{
    public class TrackableRelationTests
    {
        private IRelation<IFirstEntity, ISecondEntity> _relation;
        private IRelationChangeSet<IFirstEntity, ISecondEntity> _changes;
        private IRelation<IFirstEntity, ISecondEntity> _trackable;

        [SetUp]
        public void Setup()
        {
            _relation = A.Fake<IRelation<IFirstEntity, ISecondEntity>>();

            _changes = new RelationChangeSet<IFirstEntity, ISecondEntity>();
            _trackable = new TrackableRelation<IFirstEntity, ISecondEntity>(_changes, _relation);
        }

        [Test]
        public void TrackableAddToRelationTest()
        {
            var first = new FirstEntity();
            var second = new SecondEntity();

            _trackable.Add(first, second);

            Assert.That(_changes.HasChanges(), Is.True);
            Assert.That(_changes.Single().Action, Is.EqualTo(RelationAction.Linked));
            Assert.That(_changes.Single().Parent, Is.EqualTo(first));
            Assert.That(_changes.Single().Child, Is.EqualTo(second));
        }

        [Test]
        public void TrackableRemoveFromRelationTest()
        {
            var first = new FirstEntity();
            var second = new SecondEntity();

            _trackable.Remove(first, second);

            Assert.That(_changes.HasChanges(), Is.True);
            Assert.That(_changes.Single().Action, Is.EqualTo(RelationAction.Unlinked));
            Assert.That(_changes.Single().Parent, Is.EqualTo(first));
            Assert.That(_changes.Single().Child, Is.EqualTo(second));
        }
    }
}
