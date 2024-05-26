using CoreCraft.Core;
using CoreCraft.Exceptions;

namespace CoreCraft.Tests.Core;

public class RelationTests
{
    private IMutableRelation<FirstEntity, SecondEntity>? _relation;
    private IMapping<FirstEntity, SecondEntity>? _parentMapping;
    private IMapping<SecondEntity, FirstEntity>? _childMapping;

    [SetUp]
    public void Setup()
    {
        _parentMapping = A.Fake<IMapping<FirstEntity, SecondEntity>>();
        _childMapping = A.Fake<IMapping<SecondEntity, FirstEntity>>();

        _relation = new Relation<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo, _parentMapping, _childMapping);
    }

    [Test]
    public void RelationAddTest()
    {
        var firstEntity = new FirstEntity();
        var secondEntity = new SecondEntity();

        _relation!.Add(firstEntity, secondEntity);

        A.CallTo(() => _parentMapping!.Add(firstEntity, secondEntity)).MustHaveHappened();
        A.CallTo(() => _childMapping!.Add(secondEntity, firstEntity)).MustHaveHappened();
    }

    [Test]
    public void RelationAddOneToOneTest()
    {
        var firstEntity = new FirstEntity();
        var secondEntity = new SecondEntity();

        var relation = new Relation<FirstEntity, SecondEntity>(
            FakeModelShardInfo.OneToOneRelationInfo,
            new OneToOne<FirstEntity, SecondEntity>(),
            new OneToOne<SecondEntity, FirstEntity>());

        relation.Add(firstEntity, secondEntity);
        
        Assert.Throws<DuplicatedRelationException>(() => relation.Add(firstEntity, new()));
        Assert.Throws<DuplicatedRelationException>(() => relation.Add(new(), secondEntity));
    }

    [Test]
    public void RelationAddOneToManyTest()
    {
        var firstEntity = new FirstEntity();
        var secondEntity = new SecondEntity();

        var relation = new Relation<FirstEntity, SecondEntity>(
            FakeModelShardInfo.OneToOneRelationInfo,
            new OneToMany<FirstEntity, SecondEntity>(),
            new OneToOne<SecondEntity, FirstEntity>());

        relation.Add(firstEntity, secondEntity);

        Assert.DoesNotThrow(() => relation.Add(firstEntity, new()));
        Assert.Throws<DuplicatedRelationException>(() => relation.Add(new(), secondEntity));
    }

    [Test]
    public void RelationAddManyToOneTest()
    {
        var firstEntity = new FirstEntity();
        var secondEntity = new SecondEntity();

        var relation = new Relation<FirstEntity, SecondEntity>(
            FakeModelShardInfo.OneToOneRelationInfo,
            new OneToOne<FirstEntity, SecondEntity>(),
            new OneToMany<SecondEntity, FirstEntity>());

        relation.Add(firstEntity, secondEntity);

        Assert.Throws<DuplicatedRelationException>(() => relation.Add(firstEntity, new()));
        Assert.DoesNotThrow(() => relation.Add(new(), secondEntity));
    }

    [Test]
    public void RelationAddManyToManyTest()
    {
        var firstEntity = new FirstEntity();
        var secondEntity = new SecondEntity();

        var relation = new Relation<FirstEntity, SecondEntity>(
            FakeModelShardInfo.OneToOneRelationInfo,
            new OneToMany<FirstEntity, SecondEntity>(),
            new OneToMany<SecondEntity, FirstEntity>());

        relation.Add(firstEntity, secondEntity);

        Assert.DoesNotThrow(() => relation.Add(firstEntity, new()));
        Assert.DoesNotThrow(() => relation.Add(new(), secondEntity));
    }

    [Test]
    public void RelationRemoveTest()
    {
        var firstEntity = new FirstEntity();
        var secondEntity = new SecondEntity();

        _relation!.Remove(firstEntity, secondEntity);

        A.CallTo(() => _parentMapping!.Remove(firstEntity, secondEntity)).MustHaveHappened();
        A.CallTo(() => _childMapping!.Remove(secondEntity, firstEntity)).MustHaveHappened();
    }

    [Test]
    public void RelationManyToManyRemoveTest()
    {
        var firstEntity1 = new FirstEntity();
        var firstEntity2 = new FirstEntity();
        var secondEntity1 = new SecondEntity();
        var secondEntity2 = new SecondEntity();

        var relation = new Relation<FirstEntity, SecondEntity>(
            FakeModelShardInfo.OneToOneRelationInfo,
            new OneToMany<FirstEntity, SecondEntity>(),
            new OneToMany<SecondEntity, FirstEntity>());

        relation.Add(firstEntity1, secondEntity1);
        relation.Add(firstEntity1, secondEntity2);
        relation.Add(firstEntity2, secondEntity1);
        relation.Add(firstEntity2, secondEntity2);

        relation.Remove(firstEntity1);

        Assert.That(relation.ContainsParent(firstEntity1), Is.False);
        Assert.That(relation.ContainsParent(firstEntity2), Is.True);
        Assert.That(relation.ContainsChild(secondEntity1), Is.True);
        Assert.That(relation.ContainsChild(secondEntity2), Is.True);
        Assert.That(relation.AreLinked(firstEntity2, secondEntity1), Is.True);
        Assert.That(relation.AreLinked(firstEntity2, secondEntity2), Is.True);
    }

    [Test]
    public void RelationManyToManyRemoveAlsoRemovesChildWithNoParentsTest()
    {
        var firstEntity1 = new FirstEntity();
        var firstEntity2 = new FirstEntity();
        var secondEntity1 = new SecondEntity();
        var secondEntity2 = new SecondEntity();

        var relation = new Relation<FirstEntity, SecondEntity>(
            FakeModelShardInfo.OneToOneRelationInfo,
            new OneToMany<FirstEntity, SecondEntity>(),
            new OneToMany<SecondEntity, FirstEntity>());

        relation.Add(firstEntity1, secondEntity1);
        relation.Add(firstEntity1, secondEntity2);
        relation.Add(firstEntity2, secondEntity1);

        relation.Remove(firstEntity1);

        Assert.That(relation.ContainsParent(firstEntity1), Is.False);
        Assert.That(relation.ContainsParent(firstEntity2), Is.True);
        Assert.That(relation.ContainsChild(secondEntity1), Is.True);
        Assert.That(relation.ContainsChild(secondEntity2), Is.False);
        Assert.That(relation.AreLinked(firstEntity2, secondEntity1), Is.True);
    }

    [Test]
    public void RelationContainsParentTest()
    {
        var firstEntity = new FirstEntity();
        var secondEntity = new SecondEntity();

        _relation!.ContainsParent(firstEntity);

        A.CallTo(() => _parentMapping!.Contains(firstEntity)).MustHaveHappened();
        A.CallTo(() => _childMapping!.Contains(secondEntity)).MustNotHaveHappened();
    }

    [Test]
    public void RelationContainsChildTest()
    {
        var firstEntity = new FirstEntity();
        var secondEntity = new SecondEntity();

        _relation!.ContainsChild(secondEntity);

        A.CallTo(() => _parentMapping!.Contains(firstEntity)).MustNotHaveHappened();
        A.CallTo(() => _childMapping!.Contains(secondEntity)).MustHaveHappened();
    }

    [Test]
    public void RelationAreLinkedTest()
    {
        var firstEntity = new FirstEntity();
        var secondEntity = new SecondEntity();

        _relation!.AreLinked(firstEntity, secondEntity);

        A.CallTo(() => _parentMapping!.AreLinked(firstEntity, secondEntity)).MustHaveHappened();
        A.CallTo(() => _childMapping!.AreLinked(secondEntity, firstEntity)).MustHaveHappened();
    }

    [Test]
    public void NotMockedRelationContainsTest()
    {
        var relation = new Relation<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo, new OneToOne<FirstEntity, SecondEntity>(), new OneToMany<SecondEntity, FirstEntity>());
        var firstEntity = new FirstEntity();
        var secondEntity = new SecondEntity();

        relation.Add(firstEntity, secondEntity);

        Assert.That(relation.ContainsParent(firstEntity), Is.True);
        Assert.That(relation.ContainsChild(secondEntity), Is.True);
        Assert.That(relation.ContainsParent(new FirstEntity()), Is.False);
        Assert.That(relation.ContainsChild(new SecondEntity()), Is.False);
    }

    [Test]
    public void RelationGetChildrenTest()
    {
        var firstEntity = new FirstEntity();
        var secondEntity = new SecondEntity();

        _relation!.Children(firstEntity);

        A.CallTo(() => _parentMapping!.Children(firstEntity)).MustHaveHappened();
        A.CallTo(() => _childMapping!.Children(secondEntity)).MustNotHaveHappened();
    }

    [Test]
    public void RelationGetParentsTest()
    {
        var firstEntity = new FirstEntity();
        var secondEntity = new SecondEntity();

        _relation!.Parents(secondEntity);

        A.CallTo(() => _parentMapping!.Children(firstEntity)).MustNotHaveHappened();
        A.CallTo(() => _childMapping!.Children(secondEntity)).MustHaveHappened();
    }

    [Test]
    public void RelationCopyTest()
    {
        _relation!.Copy();

        A.CallTo(() => _parentMapping!.Copy()).MustHaveHappened();
        A.CallTo(() => _childMapping!.Copy()).MustHaveHappened();
    }

    [Test]
    public void RelationGetEnumeratorTest()
    {
        _relation!.GetEnumerator();

        A.CallTo(() => _parentMapping!.GetEnumerator()).MustHaveHappened();
        A.CallTo(() => _childMapping!.GetEnumerator()).MustNotHaveHappened();
    }

    [Test]
    public void RelationNonGenericGetEnumeratorTest()
    {
        System.Collections.IEnumerable relation = _relation!;

        relation.GetEnumerator();

        A.CallTo(() => _parentMapping!.GetEnumerator()).MustHaveHappened();
        A.CallTo(() => _childMapping!.GetEnumerator()).MustNotHaveHappened();
    }
}
