using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Tests;

public class RelationChangeSetTests
{
    [Test]
    public void LinkMultipleTimesShouldThrowExceptionTest()
    {
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>();
        var parent = new FirstEntity();
        var child = new SecondEntity();

        changes.Add(RelationAction.Linked, parent, child);

        Assert.Throws<InvalidOperationException>(() => changes.Add(RelationAction.Linked, parent, child));
    }

    [Test]
    public void UnlinkMultipleTimesShouldThrowExceptionTest()
    {
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>();
        var parent = new FirstEntity();
        var child = new SecondEntity();

        changes.Add(RelationAction.Unlinked, parent, child);

        Assert.Throws<InvalidOperationException>(() => changes.Add(RelationAction.Unlinked, parent, child));
    }

    [Test]
    public void LinkUnlinkRelationChangeSetTest()
    {
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>();
        var parent = new FirstEntity();
        var child = new SecondEntity();

        changes.Add(RelationAction.Linked, parent, child);
        changes.Add(RelationAction.Unlinked, parent, child);

        Assert.That(changes.HasChanges(), Is.False);
    }

    [Test]
    public void UnlinkLinkRelationChangeSetTest()
    {
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>();
        var parent = new FirstEntity();
        var child = new SecondEntity();

        changes.Add(RelationAction.Unlinked, parent, child);
        changes.Add(RelationAction.Linked, parent, child);

        var change = changes.Single();

        Assert.That(changes.HasChanges(), Is.True);
        Assert.That(change.Action, Is.EqualTo(RelationAction.Linked));
        Assert.That(change.Parent, Is.EqualTo(parent));
        Assert.That(change.Child, Is.EqualTo(child));
    }
}
