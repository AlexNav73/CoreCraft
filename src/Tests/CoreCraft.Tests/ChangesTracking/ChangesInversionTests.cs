using CoreCraft.ChangesTracking;

namespace CoreCraft.Tests.ChangesTracking;

public class ChangesInversionTests
{
    [Test]
    public void CollectionInvertAddChangeTest()
    {
        var entity = new FirstEntity();
        var props = new FirstEntityProperties();
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
        {
            { CollectionAction.Add, entity, null, props }
        };

        var inverted = changes.Invert();

        Assert.That(inverted.Single().Action, Is.EqualTo(CollectionAction.Remove));
        Assert.That(inverted.Single().Entity, Is.EqualTo(entity));
        Assert.That(inverted.Single().OldData, Is.EqualTo(props));
        Assert.That(inverted.Single().NewData, Is.Null);
    }

    [Test]
    public void CollectionInvertRemoveChangeTest()
    {
        var entity = new FirstEntity();
        var props = new FirstEntityProperties();
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
        {
            { CollectionAction.Remove, entity, props, null }
        };

        var inverted = changes.Invert();

        Assert.That(inverted.Single().Action, Is.EqualTo(CollectionAction.Add));
        Assert.That(inverted.Single().Entity, Is.EqualTo(entity));
        Assert.That(inverted.Single().OldData, Is.Null);
        Assert.That(inverted.Single().NewData, Is.EqualTo(props));
    }

    [Test]
    public void CollectionInvertModifyChangeTest()
    {
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
        {
            { CollectionAction.Modify, entity, oldProps, newProps }
        };

        var inverted = changes.Invert();

        Assert.That(inverted.Single().Action, Is.EqualTo(CollectionAction.Modify));
        Assert.That(inverted.Single().Entity, Is.EqualTo(entity));
        Assert.That(inverted.Single().OldData, Is.EqualTo(newProps));
        Assert.That(inverted.Single().NewData, Is.EqualTo(oldProps));
    }

    [Test]
    public void CollectionInvertInvalidActionTest()
    {
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
        {
            { (CollectionAction)42, entity, oldProps, newProps }
        };

        Assert.Throws<NotSupportedException>(() => changes.Invert());
    }

    [Test]
    public void RelationInvertLinkChangeTest()
    {
        var first = new FirstEntity();
        var second = new SecondEntity();
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>("")
        {
            { RelationAction.Linked, first, second }
        };

        var inverted = changes.Invert();

        Assert.That(inverted.Single().Action, Is.EqualTo(RelationAction.Unlinked));
        Assert.That(inverted.Single().Parent, Is.EqualTo(first));
        Assert.That(inverted.Single().Child, Is.EqualTo(second));
    }

    [Test]
    public void RelationInvertUnlinkChangeTest()
    {
        var first = new FirstEntity();
        var second = new SecondEntity();
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>("")
        {
            { RelationAction.Unlinked, first, second }
        };

        var inverted = changes.Invert();

        Assert.That(inverted.Single().Action, Is.EqualTo(RelationAction.Linked));
        Assert.That(inverted.Single().Parent, Is.EqualTo(first));
        Assert.That(inverted.Single().Child, Is.EqualTo(second));
    }

    [Test]
    public void RelationInvertInvalidActionTest()
    {
        var first = new FirstEntity();
        var second = new SecondEntity();
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>("")
        {
            { (RelationAction)42, first, second }
        };

        Assert.Throws<NotSupportedException>(() => changes.Invert());
    }
}
