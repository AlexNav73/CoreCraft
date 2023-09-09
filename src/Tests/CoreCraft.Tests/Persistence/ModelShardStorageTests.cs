using CoreCraft.ChangesTracking;
using CoreCraft.Core;
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
    public void AllCollectionsAndRelationsShouldBePassedToRepositoryTest()
    {
        var storage = new FakeModelShardStorage();
        var modelChanges = new ModelChanges();
        var changesFrame = A.Fake<IFakeChangesFrame>(c => c.Implements<IChangesFrameEx>());
        modelChanges.Register(() => (IChangesFrameEx)changesFrame);

        storage.Update(_repository!, modelChanges);

        A.CallTo(() => changesFrame.FirstCollection).MustHaveHappenedOnceExactly();
        A.CallTo(() => changesFrame.SecondCollection).MustHaveHappenedOnceExactly();
        A.CallTo(() => changesFrame.OneToOneRelation).MustHaveHappenedOnceExactly();
        A.CallTo(() => changesFrame.OneToManyRelation).MustHaveHappenedOnceExactly();
        A.CallTo(() => changesFrame.ManyToOneRelation).MustHaveHappenedOnceExactly();
        A.CallTo(() => changesFrame.ManyToManyRelation).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void UpdateCollectionTest()
    {
        var storage = new FakeModelShardStorage();
        var modelChanges = new ModelChanges();
        SetupModelChanges(modelChanges);

        storage.Update(_repository!, modelChanges);

        A.CallTo(() => _repository!.Update(
            A<CollectionInfo>.Ignored,
            A<ICollectionChangeSet<FirstEntity, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void UpdateRelationTest()
    {
        var storage = new FakeModelShardStorage();
        var modelChanges = new ModelChanges();
        SetupModelChanges(modelChanges);

        storage.Update(_repository!, modelChanges);

        A.CallTo(() => _repository!.Update(
            A<RelationInfo>.Ignored,
            A<IRelationChangeSet<FirstEntity, SecondEntity>>.Ignored))
            .MustHaveHappened(4, Times.Exactly);
    }

    [Test]
    public void SaveCollectionTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();

        storage.Save(_repository!, model);

        A.CallTo(() => _repository!.Save(
            A<CollectionInfo>.Ignored,
            A<ICollection<FirstEntity, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveRelationTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();

        storage.Save(_repository!, model);

        A.CallTo(() => _repository!.Save(
            A<RelationInfo>.Ignored,
            A<IRelation<FirstEntity, SecondEntity>>.Ignored))
            .MustHaveHappened(4, Times.Exactly);
    }

    [Test]
    public void LoadCollectionTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();

        storage.Load(_repository!, model);

        A.CallTo(() => _repository!.Load(
            A<CollectionInfo>.Ignored,
            A<IMutableCollection<FirstEntity, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LoadRelationTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();

        storage.Load(_repository!, model);

        A.CallTo(() => _repository!.Load(
            A<RelationInfo>.Ignored,
            A<IMutableRelation<FirstEntity, SecondEntity>>.Ignored,
            A<IEnumerable<FirstEntity>>.Ignored,
            A<IEnumerable<SecondEntity>>.Ignored))
            .MustHaveHappened(4, Times.Exactly);
    }

    private void SetupModelChanges(ModelChanges modelChanges)
    {
        var changesFrame = A.Fake<IFakeChangesFrame>(c => c.Implements<IChangesFrameEx>());
        modelChanges.Register(() => (IChangesFrameEx)changesFrame);
    }
}
