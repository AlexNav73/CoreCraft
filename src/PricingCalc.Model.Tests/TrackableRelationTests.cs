using PricingCalc.Model.Engine.ChangesTracking;
using PricingCalc.Model.Engine.Core;

namespace PricingCalc.Model.Tests;

public class TrackableRelationTests
{
    private IMutableRelation<FirstEntity, SecondEntity> _relation;
    private IRelationChangeSet<FirstEntity, SecondEntity> _changes;
    private IMutableRelation<FirstEntity, SecondEntity> _trackable;

    [SetUp]
    public void Setup()
    {
        _relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();

        _changes = new RelationChangeSet<FirstEntity, SecondEntity>();
        _trackable = new TrackableRelation<FirstEntity, SecondEntity>(_changes, _relation);
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
