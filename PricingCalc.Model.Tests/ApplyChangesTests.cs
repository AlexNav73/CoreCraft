using System;
using FakeItEasy;
using NUnit.Framework;
using PricingCalc.Model.Engine.ChangesTracking;
using PricingCalc.Model.Engine.Core;
using PricingCalc.Model.Tests.Infrastructure.Model.Entities;

namespace PricingCalc.Model.Tests
{
    public class ApplyChangesTests
    {
        private ICollection<IFirstEntity, IFirstEntityProperties> _collection;
        private IRelation<IFirstEntity, ISecondEntity> _relation;

        [SetUp]
        public void Setup()
        {
            _collection = A.Fake<ICollection<IFirstEntity, IFirstEntityProperties>>();
            _relation = A.Fake<IRelation<IFirstEntity, ISecondEntity>>();
        }

        [Test]
        public void ApplyAddChangeToCollectionTest()
        {
            var changes = new CollectionChangeSet<IFirstEntity, IFirstEntityProperties>();
            var entity = new FirstEntity();
            var props = A.Fake<IFirstEntityProperties>();
            var factory = A.Fake<IFactory<IFirstEntity, IFirstEntityProperties>>();

            Guid? expectedId = null;

            A.CallTo(() => _collection.Create())
                .Returns(new EntityBuilder<IFirstEntity, IFirstEntityProperties>((e, p) => expectedId = e.Id, factory));

            changes.Add(CollectionAction.Add, entity, null, props);

            changes.Apply(_collection);

            A.CallTo(() => _collection.Create()).MustHaveHappened();
            Assert.That(expectedId, Is.EqualTo(entity.Id));
        }

        [Test]
        public void ApplyRemoveChangeToCollectionTest()
        {
            var changes = new CollectionChangeSet<IFirstEntity, IFirstEntityProperties>();
            var entity = A.Fake<IFirstEntity>();
            var props = A.Fake<IFirstEntityProperties>();

            changes.Add(CollectionAction.Remove, entity, props, null);

            changes.Apply(_collection);

            A.CallTo(() => _collection.Remove(entity)).MustHaveHappened();
        }

        [Test]
        public void ApplyModifyChangeToCollectionTest()
        {
            var changes = new CollectionChangeSet<IFirstEntity, IFirstEntityProperties>();
            var entity = A.Fake<IFirstEntity>();
            var oldProps = A.Fake<IFirstEntityProperties>();
            var newProps = A.Fake<IFirstEntityProperties>();

            changes.Add(CollectionAction.Modify, entity, oldProps, newProps);

            changes.Apply(_collection);

            A.CallTo(() => _collection.Modify(entity, A<Action<IFirstEntityProperties>>.Ignored))
                .MustHaveHappened();
        }

        [Test]
        public void ApplyLinkChangeToRelationTest()
        {
            var changes = new RelationChangeSet<IFirstEntity, ISecondEntity>();
            var parent = A.Fake<IFirstEntity>();
            var child = A.Fake<ISecondEntity>();

            changes.Add(RelationAction.Linked, parent, child);

            changes.Apply(_relation);

            A.CallTo(() => _relation.Add(parent, child)).MustHaveHappened();
        }

        [Test]
        public void ApplyUnlinkChangeToRelationTest()
        {
            var changes = new RelationChangeSet<IFirstEntity, ISecondEntity>();
            var parent = A.Fake<IFirstEntity>();
            var child = A.Fake<ISecondEntity>();

            changes.Add(RelationAction.Unlinked, parent, child);

            changes.Apply(_relation);

            A.CallTo(() => _relation.Remove(parent, child)).MustHaveHappened();
        }
    }
}
