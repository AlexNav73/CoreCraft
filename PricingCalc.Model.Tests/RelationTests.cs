using FakeItEasy;
using NUnit.Framework;
using PricingCalc.Model.Engine.Core;
using PricingCalc.Model.Tests.Model.Entities;

namespace PricingCalc.Model.Tests
{
    public class RelationTests
    {
        private IRelation<IFirstEntity, ISecondEntity> _relation;
        private IMapping<IFirstEntity, ISecondEntity> _parentMapping;
        private IMapping<ISecondEntity, IFirstEntity> _childMapping;

        [SetUp]
        public void Setup()
        {
            _parentMapping = A.Fake<IMapping<IFirstEntity, ISecondEntity>>();
            _childMapping = A.Fake<IMapping<ISecondEntity, IFirstEntity>>();

            _relation = new Relation<IFirstEntity, ISecondEntity>(_parentMapping, _childMapping);
        }

        [Test]
        public void RelationAddTest()
        {
            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();

            _relation.Add(firstEntity, secondEntity);

            A.CallTo(() => _parentMapping.Add(firstEntity, secondEntity)).MustHaveHappened();
            A.CallTo(() => _childMapping.Add(secondEntity, firstEntity)).MustHaveHappened();
        }

        [Test]
        public void RelationRemoveTest()
        {
            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();

            _relation.Remove(firstEntity, secondEntity);

            A.CallTo(() => _parentMapping.Remove(firstEntity, secondEntity)).MustHaveHappened();
            A.CallTo(() => _childMapping.Remove(secondEntity, firstEntity)).MustHaveHappened();
        }

        [Test]
        public void RelationGetChildrenTest()
        {
            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();

            var children = _relation.Children(firstEntity);

            A.CallTo(() => _parentMapping.Children(firstEntity)).MustHaveHappened();
            A.CallTo(() => _childMapping.Children(secondEntity)).MustNotHaveHappened();
        }

        [Test]
        public void RelationGetParentsTest()
        {
            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();

            var children = _relation.Parents(secondEntity);

            A.CallTo(() => _parentMapping.Children(firstEntity)).MustNotHaveHappened();
            A.CallTo(() => _childMapping.Children(secondEntity)).MustHaveHappened();
        }

        [Test]
        public void RelationCopyTest()
        {
            var copy = _relation.Copy();

            A.CallTo(() => _parentMapping.Copy()).MustHaveHappened();
            A.CallTo(() => _childMapping.Copy()).MustHaveHappened();
        }

        [Test]
        public void RelationEnumeratorTest()
        {
            var copy = _relation.GetEnumerator();

            A.CallTo(() => _parentMapping.GetEnumerator()).MustHaveHappened();
            A.CallTo(() => _childMapping.GetEnumerator()).MustNotHaveHappened();
        }
    }
}
