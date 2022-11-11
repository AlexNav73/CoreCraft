using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Tests;

public class CollectionChangeSetTests
{
    [Test]
    public void AddMultipleTimesShouldThrowExceptionTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var entity = new FirstEntity();
        var props = new FirstEntityProperties();

        changes.Add(CollectionAction.Add, entity, null, props);

        Assert.Throws<InvalidOperationException>(() => changes.Add(CollectionAction.Add, entity, null, props));
    }

    [Test]
    public void ModifyAddCollectionChangeSetTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();

        changes.Add(CollectionAction.Modify, entity, oldProps, newProps);

        Assert.Throws<InvalidOperationException>(() => changes.Add(CollectionAction.Add, entity, null, newProps));
    }

    [Test]
    public void RemoveModifyCollectionChangeSetTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();

        changes.Add(CollectionAction.Remove, entity, oldProps, null);

        Assert.Throws<InvalidOperationException>(() => changes.Add(CollectionAction.Modify, entity, oldProps, newProps));
    }

    [Test]
    public void RemoveMultipleTimesShouldThrowExceptionTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();

        changes.Add(CollectionAction.Remove, entity, oldProps, null);

        Assert.Throws<InvalidOperationException>(() => changes.Add(CollectionAction.Remove, entity, oldProps, null));
    }

    [Test]
    public void AddModifyCollectionChangeSetTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();

        changes.Add(CollectionAction.Add, entity, null, newProps);
        changes.Add(CollectionAction.Modify, entity, oldProps, newProps);

        var change = changes.Single();

        Assert.That(change.Action, Is.EqualTo(CollectionAction.Add));
        Assert.That(change.Entity, Is.EqualTo(entity));
        Assert.That(change.OldData, Is.Null);
        Assert.That(change.NewData, Is.EqualTo(newProps));
    }

    [Test]
    public void ModifyModifyCollectionChangeSetTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();

        changes.Add(CollectionAction.Modify, entity, oldProps, newProps);
        changes.Add(CollectionAction.Modify, entity, oldProps, newProps);

        var change = changes.Single();

        Assert.That(change.Action, Is.EqualTo(CollectionAction.Modify));
        Assert.That(change.Entity, Is.EqualTo(entity));
        Assert.That(change.OldData, Is.EqualTo(oldProps));
        Assert.That(change.NewData, Is.EqualTo(newProps));
    }

    [Test]
    public void AddModifyRemoveCollectionChangeSetTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();

        changes.Add(CollectionAction.Add, entity, null, newProps);
        changes.Add(CollectionAction.Modify, entity, oldProps, newProps);
        changes.Add(CollectionAction.Remove, entity, oldProps, null);

        Assert.That(changes.Count(), Is.EqualTo(0));
    }

    [Test]
    public void AddModifyModifyRemoveCollectionChangeSetTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();

        changes.Add(CollectionAction.Add, entity, null, newProps);
        changes.Add(CollectionAction.Modify, entity, oldProps, newProps);
        changes.Add(CollectionAction.Modify, entity, oldProps, newProps);
        changes.Add(CollectionAction.Remove, entity, oldProps, null);

        Assert.That(changes.Count(), Is.EqualTo(0));
    }

    [Test]
    public void AddRemoveCollectionChangeSetTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();

        changes.Add(CollectionAction.Add, entity, null, newProps);
        changes.Add(CollectionAction.Remove, entity, oldProps, null);

        Assert.That(changes.Count(), Is.EqualTo(0));
    }
}
