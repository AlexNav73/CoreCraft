using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Exceptions;
using CoreCraft.Persistence;

namespace CoreCraft.Tests.ChangesTracking;

public class RelationChangeSetTests
{
    [Test]
    public void LinkMultipleTimesShouldThrowExceptionTest()
    {
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo);
        var parent = new FirstEntity();
        var child = new SecondEntity();

        changes.Add(RelationAction.Linked, parent, child);

        var exception = Assert.Throws<InvalidChangeSequenceException>(() => changes.Add(RelationAction.Linked, parent, child));
        var prevChange = exception!.PreviousChange as IRelationChange<FirstEntity, SecondEntity>;
        var nextChange = exception!.NextChange as IRelationChange<FirstEntity, SecondEntity>;

        Assert.That(prevChange, Is.Not.Null);
        Assert.That(prevChange!.Action, Is.EqualTo(RelationAction.Linked));
        Assert.That(prevChange!.Parent, Is.EqualTo(parent));
        Assert.That(prevChange!.Child, Is.EqualTo(child));

        Assert.That(nextChange, Is.Not.Null);
        Assert.That(nextChange!.Action, Is.EqualTo(RelationAction.Linked));
        Assert.That(nextChange!.Parent, Is.EqualTo(parent));
        Assert.That(nextChange!.Child, Is.EqualTo(child));
    }

    [Test]
    public void UnlinkMultipleTimesShouldThrowExceptionTest()
    {
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo);
        var parent = new FirstEntity();
        var child = new SecondEntity();

        changes.Add(RelationAction.Unlinked, parent, child);

        var exception = Assert.Throws<InvalidChangeSequenceException>(() => changes.Add(RelationAction.Unlinked, parent, child));
        var prevChange = exception!.PreviousChange as IRelationChange<FirstEntity, SecondEntity>;
        var nextChange = exception!.NextChange as IRelationChange<FirstEntity, SecondEntity>;

        Assert.That(prevChange, Is.Not.Null);
        Assert.That(prevChange!.Action, Is.EqualTo(RelationAction.Unlinked));
        Assert.That(prevChange!.Parent, Is.EqualTo(parent));
        Assert.That(prevChange!.Child, Is.EqualTo(child));

        Assert.That(nextChange, Is.Not.Null);
        Assert.That(nextChange!.Action, Is.EqualTo(RelationAction.Unlinked));
        Assert.That(prevChange!.Parent, Is.EqualTo(parent));
        Assert.That(prevChange!.Child, Is.EqualTo(child));
    }

    [Test]
    public void LinkUnlinkRelationChangeSetTest()
    {
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo);
        var parent = new FirstEntity();
        var child = new SecondEntity();

        changes.Add(RelationAction.Linked, parent, child);
        changes.Add(RelationAction.Unlinked, parent, child);

        Assert.That(changes.HasChanges(), Is.False);
    }

    [Test]
    public void UnlinkLinkRelationChangeSetTest()
    {
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo );
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

    [Test]
    public void MergeRelationChangeSetTest()
    {
        var changes1 = new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo);
        var changes2 = new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo);
        var parent = new FirstEntity();
        var child = new SecondEntity();

        changes1.Add(RelationAction.Linked, parent, child);
        changes2.Add(RelationAction.Unlinked, parent, child);

        var merged = changes1.Merge(changes2);

        Assert.That(merged.Count(), Is.EqualTo(0));
        Assert.That(merged.HasChanges(), Is.False);
    }

    [Test]
    public void NonGenericGetEnumeratorTest()
    {
        System.Collections.IEnumerable relation = new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo);

        var enumerator = relation.GetEnumerator();

        Assert.That(enumerator, Is.Not.Null);
        Assert.That(enumerator.MoveNext(), Is.False);
    }

    [Test]
    public void AddChangeWithInvalidActionShouldThrowExceptionTest()
    {
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo);
        var first = new FirstEntity();
        var second = new SecondEntity();

        changes.Add((RelationAction)42, first, second);

        Assert.Throws<NotSupportedException>(() => changes.Apply(A.Fake<IMutableRelation<FirstEntity, SecondEntity>>()));
    }

    [Test]
    public void SaveShouldCallRepositoryTest()
    {
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>(FakeModelShardInfo.OneToOneRelationInfo);
        var repo = A.Fake<IRepository>();

        changes.Save(repo);

        A.CallTo(() => repo.Save(A<IRelationChangeSet<FirstEntity, SecondEntity>>.Ignored))
            .Invokes(c =>
            {
                var changeSet = c.Arguments[0];

                Assert.That(ReferenceEquals(changes, changeSet), Is.True);
            });
    }
}
