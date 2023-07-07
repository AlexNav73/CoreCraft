using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Exceptions;
using CoreCraft.Persistence;

namespace CoreCraft.Tests.Persistence;

public class ModelShardStorageTests
{
    private IRepository? _repository;

    [SetUp]
    public void SetUp()
    {
        _repository = A.Fake<IRepository>();
    }

    [Test]
    public void UpdateWithoutChangesTest()
    {
        var storage = new FakeModelShardStorage();
        var modelChanges = new ModelChanges();
        var changesFrame = A.Fake<IFakeChangesFrame>(c => c.Implements<IChangesFrameEx>());
        modelChanges.Register(() => (IChangesFrameEx)changesFrame);

        storage.Update(_repository!, modelChanges);

        A.CallTo(() => changesFrame.FirstCollection.HasChanges()).MustHaveHappenedOnceExactly();
        A.CallTo(() => changesFrame.SecondCollection.HasChanges()).MustHaveHappenedOnceExactly();
        A.CallTo(() => changesFrame.OneToOneRelation.HasChanges()).MustHaveHappenedOnceExactly();
        A.CallTo(() => changesFrame.OneToManyRelation.HasChanges()).MustHaveHappenedOnceExactly();
        A.CallTo(() => changesFrame.ManyToOneRelation.HasChanges()).MustHaveHappenedOnceExactly();
        A.CallTo(() => changesFrame.ManyToManyRelation.HasChanges()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void UpdateCollectionAddChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var modelChanges = new ModelChanges();
        SetupModelChanges(modelChanges, CollectionAction.Add, null, new());

        storage.Update(_repository!, modelChanges);

        A.CallTo(() => _repository!.Insert(
            A<CollectionInfo>.Ignored,
            A<IReadOnlyCollection<KeyValuePair<FirstEntity, FirstEntityProperties>>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void UpdateCollectionRemoveChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var modelChanges = new ModelChanges();
        SetupModelChanges(modelChanges, CollectionAction.Remove, new(), null);

        storage.Update(_repository!, modelChanges);

        A.CallTo(() => _repository!.Delete(
            A<CollectionInfo>.Ignored,
            A<IReadOnlyCollection<FirstEntity>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void UpdateCollectionModifyChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var modelChanges = new ModelChanges();
        SetupModelChanges(modelChanges, CollectionAction.Modify, new(), new());

        storage.Update(_repository!, modelChanges);

        A.CallTo(() => _repository!.Update(
            A<CollectionInfo>.Ignored,
            A<IReadOnlyCollection<ICollectionChange<FirstEntity, FirstEntityProperties>>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void UpdateRelationLinkChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var modelChanges = new ModelChanges();
        SetupModelChanges(modelChanges, RelationAction.Linked);

        storage.Update(_repository!, modelChanges);

        A.CallTo(() => _repository!.Insert(
            A<RelationInfo>.Ignored,
            A<IReadOnlyCollection<KeyValuePair<FirstEntity, SecondEntity>>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void UpdateRelationUnlinkChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var modelChanges = new ModelChanges();
        SetupModelChanges(modelChanges, RelationAction.Unlinked);

        storage.Update(_repository!, modelChanges);

        A.CallTo(() => _repository!.Delete(
            A<RelationInfo>.Ignored,
            A<IReadOnlyCollection<KeyValuePair<FirstEntity, SecondEntity>>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveCollectionChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();

        storage.Save(_repository!, model);

        A.CallTo(() => _repository!.Insert(
            A<CollectionInfo>.Ignored,
            A<IReadOnlyCollection<KeyValuePair<FirstEntity, FirstEntityProperties>>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveRelationChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();

        storage.Save(_repository!, model);

        A.CallTo(() => _repository!.Insert(
            A<RelationInfo>.Ignored,
            A<IReadOnlyCollection<KeyValuePair<FirstEntity, SecondEntity>>>.Ignored))
            .MustHaveHappened(4, Times.Exactly);
    }

    [Test]
    public void LoadCollectionChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();

        storage.Load(_repository!, model);

        A.CallTo(() => _repository!.Select(
            A<CollectionInfo>.Ignored,
            A<IMutableCollection<FirstEntity, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LoadCollectionDataToNonEmptyModelTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();
        var shard = A.Fake<IMutableFakeModelShard>();
        var collection = A.Fake<IMutableCollection<FirstEntity, FirstEntityProperties>>();

        A.CallTo(() => model.Shard<IMutableFakeModelShard>()).Returns(shard);
        A.CallTo(() => shard.FirstCollection).Returns(collection);
        A.CallTo(() => collection.Count).Returns(1);

        Assert.Throws<NonEmptyModelException>(() => storage.Load(_repository!, model));
    }

    [Test]
    public void LoadRelationChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();

        storage.Load(_repository!, model);

        A.CallTo(() => _repository!.Select(
            A<RelationInfo>.Ignored,
            A<IMutableRelation<FirstEntity, SecondEntity>>.Ignored,
            A<IEnumerable<FirstEntity>>.Ignored,
            A<IEnumerable<SecondEntity>>.Ignored))
            .MustHaveHappened(4, Times.Exactly);
    }

    [Test]
    public void LoadRelationDataToNonEmptyModelTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();
        var shard = A.Fake<IMutableFakeModelShard>();
        var relation = A.Fake<IMutableRelation<FirstEntity, SecondEntity>>();
        var relations = new List<FirstEntity>() { new FirstEntity() };

        A.CallTo(() => model.Shard<IMutableFakeModelShard>()).Returns(shard);
        A.CallTo(() => shard.OneToOneRelation).Returns(relation);
        A.CallTo(() => relation.GetEnumerator()).Returns(relations.GetEnumerator());

        Assert.Throws<NonEmptyModelException>(() => storage.Load(_repository!, model));
    }

    private void SetupModelChanges(ModelChanges modelChanges, CollectionAction action, FirstEntityProperties? oldProps, FirstEntityProperties? newProps)
    {
        var changesFrame = A.Fake<IFakeChangesFrame>(c => c.Implements<IChangesFrameEx>());
        A.CallTo(() => changesFrame.FirstCollection.HasChanges()).Returns(true);
        A.CallTo(() => changesFrame.FirstCollection.GetEnumerator())
            .Returns(new List<CollectionChange<FirstEntity, FirstEntityProperties>>()
            {
                new CollectionChange<FirstEntity, FirstEntityProperties>(action, new(), oldProps, newProps)
            }.GetEnumerator());
        modelChanges.Register(() => (IChangesFrameEx)changesFrame);
    }

    private void SetupModelChanges(ModelChanges modelChanges, RelationAction action)
    {
        var changesFrame = A.Fake<IFakeChangesFrame>(c => c.Implements<IChangesFrameEx>());
        A.CallTo(() => changesFrame.OneToManyRelation.HasChanges()).Returns(true);
        A.CallTo(() => changesFrame.OneToManyRelation.GetEnumerator())
            .Returns(new List<RelationChange<FirstEntity, SecondEntity>>()
            {
                new RelationChange<FirstEntity, SecondEntity>(action, new(), new())
            }.GetEnumerator());
        modelChanges.Register(() => (IChangesFrameEx)changesFrame);
    }
}
