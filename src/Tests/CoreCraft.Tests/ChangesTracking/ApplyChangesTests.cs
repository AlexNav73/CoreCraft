using CoreCraft.ChangesTracking;
using CoreCraft.Core;

namespace CoreCraft.Tests.ChangesTracking;

public class ApplyChangesTests
{
    private IMutableCollection<FirstEntity, FirstEntityProperties>? _collection;
    private IMutableRelation<FirstEntity, SecondEntity>? _relation;

    [SetUp]
    public void Setup()
    {
        _collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();
        _relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
    }

    [Test]
    public void ApplyAddChangeToCollectionTest()
    {
        var entity = new FirstEntity();
        var props = new FirstEntityProperties();
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
        {
            { CollectionAction.Add, entity, null, props }
        };

        changes.Apply(_collection!);

        A.CallTo(() => _collection!.Add(entity, props)).MustHaveHappened();
    }

    [Test]
    public void ApplyRemoveChangeToCollectionTest()
    {
        var entity = new FirstEntity();
        var props = new FirstEntityProperties();
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
        {
            { CollectionAction.Remove, entity, props, null }
        };

        changes.Apply(_collection!);

        A.CallTo(() => _collection!.Remove(entity)).MustHaveHappened();
    }

    [Test]
    public void ApplyModifyChangeToCollectionTest()
    {
        var entity = new FirstEntity();
        var oldProps = new FirstEntityProperties();
        var newProps = new FirstEntityProperties();
        var changes = new CollectionChangeSet<FirstEntity, FirstEntityProperties>("")
        {
            { CollectionAction.Modify, entity, oldProps, newProps }
        };

        changes.Apply(_collection!);

        A.CallTo(() => _collection!.Modify(entity, A<Func<FirstEntityProperties, FirstEntityProperties>>.Ignored))
            .MustHaveHappened();
    }

    [Test]
    public void ApplyLinkChangeToRelationTest()
    {
        var parent = new FirstEntity();
        var child = new SecondEntity();
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>("")
        {
            { RelationAction.Linked, parent, child }
        };

        changes.Apply(_relation!);

        A.CallTo(() => _relation!.Add(parent, child)).MustHaveHappened();
    }

    [Test]
    public void ApplyUnlinkChangeToRelationTest()
    {
        var parent = new FirstEntity();
        var child = new SecondEntity();
        var changes = new RelationChangeSet<FirstEntity, SecondEntity>("")
        {
            { RelationAction.Unlinked, parent, child }
        };

        changes.Apply(_relation!);

        A.CallTo(() => _relation!.Remove(parent, child)).MustHaveHappened();
    }
}
