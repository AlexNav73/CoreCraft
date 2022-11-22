using System.Collections;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Exceptions;

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

        var exception = Assert.Throws<InvalidChangeSequenceException>(() => changes.Add(CollectionAction.Add, entity, null, props));
        var prevChange = exception!.PreviousChange as ICollectionChange<FirstEntity, FirstEntityProperties>;
        var nextChange = exception!.NextChange as ICollectionChange<FirstEntity, FirstEntityProperties>;

        Assert.That(prevChange, Is.Not.Null);
        Assert.That(prevChange!.Action, Is.EqualTo(CollectionAction.Add));
        Assert.That(prevChange!.OldData, Is.Null);
        Assert.That(prevChange!.NewData, Is.EqualTo(props));

        Assert.That(nextChange, Is.Not.Null);
        Assert.That(nextChange!.Action, Is.EqualTo(CollectionAction.Add));
        Assert.That(nextChange!.OldData, Is.Null);
        Assert.That(nextChange!.NewData, Is.EqualTo(props));
    }

    [Test]
    public void ModifyAddCollectionChangeSetTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();

        changes.Add(CollectionAction.Modify, entity, oldProps, newProps);

        var exception = Assert.Throws<InvalidChangeSequenceException>(() => changes.Add(CollectionAction.Add, entity, null, newProps));
        var prevChange = exception!.PreviousChange as ICollectionChange<FirstEntity, FirstEntityProperties>;
        var nextChange = exception!.NextChange as ICollectionChange<FirstEntity, FirstEntityProperties>;

        Assert.That(prevChange, Is.Not.Null);
        Assert.That(prevChange!.Action, Is.EqualTo(CollectionAction.Modify));
        Assert.That(prevChange!.OldData, Is.EqualTo(oldProps));
        Assert.That(prevChange!.NewData, Is.EqualTo(newProps));

        Assert.That(nextChange, Is.Not.Null);
        Assert.That(nextChange!.Action, Is.EqualTo(CollectionAction.Add));
        Assert.That(nextChange!.OldData, Is.Null);
        Assert.That(nextChange!.NewData, Is.EqualTo(newProps));
    }

    [Test]
    public void RemoveModifyCollectionChangeSetTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();

        changes.Add(CollectionAction.Remove, entity, oldProps, null);

        var exception = Assert.Throws<InvalidChangeSequenceException>(() => changes.Add(CollectionAction.Modify, entity, oldProps, newProps));
        var prevChange = exception!.PreviousChange as ICollectionChange<FirstEntity, FirstEntityProperties>;
        var nextChange = exception!.NextChange as ICollectionChange<FirstEntity, FirstEntityProperties>;

        Assert.That(prevChange, Is.Not.Null);
        Assert.That(prevChange!.Action, Is.EqualTo(CollectionAction.Remove));
        Assert.That(prevChange!.OldData, Is.EqualTo(oldProps));
        Assert.That(prevChange!.NewData, Is.Null);

        Assert.That(nextChange, Is.Not.Null);
        Assert.That(nextChange!.Action, Is.EqualTo(CollectionAction.Modify));
        Assert.That(nextChange!.OldData, Is.EqualTo(oldProps));
        Assert.That(nextChange!.NewData, Is.EqualTo(newProps));
    }

    [Test]
    public void RemoveMultipleTimesShouldThrowExceptionTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();

        changes.Add(CollectionAction.Remove, entity, oldProps, null);

        var exception = Assert.Throws<InvalidChangeSequenceException>(() => changes.Add(CollectionAction.Remove, entity, oldProps, null));
        var prevChange = exception!.PreviousChange as ICollectionChange<FirstEntity, FirstEntityProperties>;
        var nextChange = exception!.NextChange as ICollectionChange<FirstEntity, FirstEntityProperties>;

        Assert.That(prevChange, Is.Not.Null);
        Assert.That(prevChange!.Action, Is.EqualTo(CollectionAction.Remove));
        Assert.That(prevChange!.OldData, Is.EqualTo(oldProps));
        Assert.That(prevChange!.NewData, Is.Null);

        Assert.That(nextChange, Is.Not.Null);
        Assert.That(nextChange!.Action, Is.EqualTo(CollectionAction.Remove));
        Assert.That(nextChange!.OldData, Is.EqualTo(oldProps));
        Assert.That(nextChange!.NewData, Is.Null);
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
    public void RemoveAddCollectionChangeSetTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();

        changes.Add(CollectionAction.Remove, entity, oldProps, null);
        changes.Add(CollectionAction.Add, entity, null, newProps);

        var change = changes.Single();

        Assert.That(change.Action, Is.EqualTo(CollectionAction.Modify));
        Assert.That(change.Entity, Is.EqualTo(entity));
        Assert.That(change.OldData, Is.EqualTo(oldProps));
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

    [Test]
    public void MergeCollectionChangeSetTest()
    {
        var changes1 = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var changes2 = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();

        changes1.Add(CollectionAction.Add, entity, null, newProps);
        changes2.Add(CollectionAction.Remove, entity, oldProps, null);

        var merged = changes1.Merge(changes2);

        Assert.That(merged.Count(), Is.EqualTo(0));
        Assert.That(merged.HasChanges(), Is.False);
    }

    [Test]
    public void GetEnumeratorCollectionChangeSetTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();

        var enumerator = changes.GetEnumerator();

        Assert.That(enumerator, Is.Not.Null);
    }

    [Test]
    public void GetNonGenericEnumeratorCollectionChangeSetTest()
    {
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>();

        var enumerator = ((IEnumerable)changes).GetEnumerator();

        Assert.That(enumerator, Is.Not.Null);
    }
}
