using System;
using System.Linq;
using NUnit.Framework;
using PricingCalc.Model.Engine.Core;
using PricingCalc.Model.Tests.Model.Entities;

namespace PricingCalc.Model.Tests
{
    public class MappingsTests
    {
        [Test]
        public void OneToOneAddRelationTest()
        {
            var mapping = new OneToOne<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();

            mapping.Add(firstEntity, secondEntity);

            Assert.That(mapping.Single(), Is.EqualTo(firstEntity));
        }

        [Test]
        public void OneToOneAddSecondChildToRelationTest()
        {
            var mapping = new OneToOne<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();
            var thirdEntity = new SecondEntity();

            mapping.Add(firstEntity, secondEntity);

            Assert.Throws<InvalidOperationException>(() => mapping.Add(firstEntity, thirdEntity));
        }

        [Test]
        public void OneToManyAddRelationTest()
        {
            var mapping = new OneToMany<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();

            mapping.Add(firstEntity, secondEntity);

            Assert.That(mapping.Single(), Is.EqualTo(firstEntity));
        }

        [Test]
        public void OneToManyAddSecondChildToRelationTest()
        {
            var mapping = new OneToMany<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();
            var thirdEntity = new SecondEntity();

            mapping.Add(firstEntity, secondEntity);

            Assert.DoesNotThrow(() => mapping.Add(firstEntity, thirdEntity));
        }

        [Test]
        public void OneToOneGetTest()
        {
            var mapping = new OneToOne<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();

            mapping.Add(firstEntity, secondEntity);

            Assert.That(mapping.Children(firstEntity), Is.EqualTo(new[] { secondEntity }));
        }

        [Test]
        public void OneToManyGetTest()
        {
            var mapping = new OneToMany<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();
            var thirdEntity = new SecondEntity();

            mapping.Add(firstEntity, secondEntity);
            mapping.Add(firstEntity, thirdEntity);

            Assert.That(mapping.Children(firstEntity), Is.EqualTo(new[] { secondEntity, thirdEntity }));
        }

        [Test]
        public void OneToOneRemoveTest()
        {
            var mapping = new OneToOne<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();

            mapping.Add(firstEntity, secondEntity);
            mapping.Remove(firstEntity, secondEntity);

            Assert.That(mapping, Is.Empty);
        }

        [Test]
        public void OneToOneRemoveFromEmptyReltionTest()
        {
            var mapping = new OneToOne<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();

            Assert.Throws<InvalidOperationException>(() => mapping.Remove(firstEntity, secondEntity));
        }

        [Test]
        public void OneToOneRemoveInvalidEntityTest()
        {
            var mapping = new OneToOne<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();
            var thirdEntity = new FirstEntity();

            mapping.Add(firstEntity, secondEntity);

            Assert.Throws<InvalidOperationException>(() => mapping.Remove(thirdEntity, secondEntity));
        }

        [Test]
        public void OneToManyRemoveTest()
        {
            var mapping = new OneToMany<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();

            mapping.Add(firstEntity, secondEntity);
            mapping.Remove(firstEntity, secondEntity);

            Assert.That(mapping, Is.Empty);
        }

        [Test]
        public void OneToManyRemoveSomeItemsTest()
        {
            var mapping = new OneToMany<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();
            var thirdEntity = new SecondEntity();

            mapping.Add(firstEntity, secondEntity);
            mapping.Add(firstEntity, thirdEntity);
            mapping.Remove(firstEntity, secondEntity);
            mapping.Remove(firstEntity, thirdEntity);

            Assert.That(mapping, Is.Empty);
        }

        [Test]
        public void OneToManyRemoveFromEmptyReltionTest()
        {
            var mapping = new OneToMany<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();

            Assert.Throws<InvalidOperationException>(() => mapping.Remove(firstEntity, secondEntity));
        }

        [Test]
        public void OneToManyRemoveInvalidEntityTest()
        {
            var mapping = new OneToMany<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();
            var thirdEntity = new FirstEntity();

            mapping.Add(firstEntity, secondEntity);

            Assert.Throws<InvalidOperationException>(() => mapping.Remove(thirdEntity, secondEntity));
        }

        [Test]
        public void OneToOneCopyTest()
        {
            var mapping = new OneToOne<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();

            mapping.Add(firstEntity, secondEntity);

            var copy = mapping.Copy();

            Assert.That(ReferenceEquals(mapping, copy), Is.False);
            Assert.That(copy.Children(firstEntity), Is.EquivalentTo(mapping.Children(firstEntity)));
        }

        [Test]
        public void OneToManyCopyTest()
        {
            var mapping = new OneToMany<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();
            var thirdEntity = new SecondEntity();

            mapping.Add(firstEntity, secondEntity);
            mapping.Add(firstEntity, thirdEntity);

            var copy = mapping.Copy();

            Assert.That(ReferenceEquals(mapping, copy), Is.False);
            Assert.That(copy.Children(firstEntity), Is.EquivalentTo(mapping.Children(firstEntity)));
        }

        [Test]
        public void OneToOneEnumeratorTest()
        {
            var mapping = new OneToOne<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();

            mapping.Add(firstEntity, secondEntity);

            var entities = mapping.ToArray();

            Assert.That(entities, Is.EquivalentTo(new[] { firstEntity }));
        }

        [Test]
        public void OneToManyEnumeratorTest()
        {
            var mapping = new OneToMany<IFirstEntity, ISecondEntity>();

            var firstEntity = new FirstEntity();
            var secondEntity = new SecondEntity();
            var thirdEntity = new SecondEntity();

            mapping.Add(firstEntity, secondEntity);
            mapping.Add(firstEntity, thirdEntity);

            var entities = mapping.ToArray();

            Assert.That(entities, Is.EquivalentTo(new[] { firstEntity }));
        }
    }
}
