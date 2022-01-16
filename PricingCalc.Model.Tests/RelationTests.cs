using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Tests;

public class RelationTests
{
    private IMutableRelation<FirstEntity, SecondEntity> _relation;
    private IMapping<FirstEntity, SecondEntity> _parentMapping;
    private IMapping<SecondEntity, FirstEntity> _childMapping;

    [SetUp]
    public void Setup()
    {
        _parentMapping = A.Fake<IMapping<FirstEntity, SecondEntity>>();
        _childMapping = A.Fake<IMapping<SecondEntity, FirstEntity>>();

        _relation = new Relation<FirstEntity, SecondEntity>(_parentMapping, _childMapping);
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

        _relation.Children(firstEntity);

        A.CallTo(() => _parentMapping.Children(firstEntity)).MustHaveHappened();
        A.CallTo(() => _childMapping.Children(secondEntity)).MustNotHaveHappened();
    }

    [Test]
    public void RelationGetParentsTest()
    {
        var firstEntity = new FirstEntity();
        var secondEntity = new SecondEntity();

        _relation.Parents(secondEntity);

        A.CallTo(() => _parentMapping.Children(firstEntity)).MustNotHaveHappened();
        A.CallTo(() => _childMapping.Children(secondEntity)).MustHaveHappened();
    }

    [Test]
    public void RelationCopyTest()
    {
        _relation.Copy();

        A.CallTo(() => _parentMapping.Copy()).MustHaveHappened();
        A.CallTo(() => _childMapping.Copy()).MustHaveHappened();
    }

    [Test]
    public void RelationEnumeratorTest()
    {
        _relation.GetEnumerator();

        A.CallTo(() => _parentMapping.GetEnumerator()).MustHaveHappened();
        A.CallTo(() => _childMapping.GetEnumerator()).MustNotHaveHappened();
    }
}
