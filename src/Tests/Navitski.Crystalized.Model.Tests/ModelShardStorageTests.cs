using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Persistence;

namespace Navitski.Crystalized.Model.Tests;

public class ModelShardStorageTests
{
    private IRepository? _repository;

    [SetUp]
    public void SetUp()
    {
        _repository = A.Fake<IRepository>();
    }

    [Test]
    public void MigrateWithoutChangesTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();
        var modelChanges = new ModelChanges();
        var changesFrame = A.Fake<IWritableChangesFrame>(c => c.Implements<IFakeChangesFrame>());
        modelChanges.Register(changesFrame);

        storage.Migrate(_repository!, model, modelChanges);

        A.CallTo(() => ((IFakeChangesFrame)changesFrame).FirstCollection.HasChanges()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IFakeChangesFrame)changesFrame).SecondCollection.HasChanges()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IFakeChangesFrame)changesFrame).OneToOneRelation.HasChanges()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IFakeChangesFrame)changesFrame).OneToManyRelation.HasChanges()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IFakeChangesFrame)changesFrame).ManyToOneRelation.HasChanges()).MustHaveHappenedOnceExactly();
        A.CallTo(() => ((IFakeChangesFrame)changesFrame).ManyToManyRelation.HasChanges()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void MigrateCollectionAddChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();
        var modelChanges = new ModelChanges();
        SetupModelChanges(modelChanges, CollectionAction.Add, null, new());

        storage.Migrate(_repository!, model, modelChanges);

        A.CallTo(() => _repository!.Insert(
            A<string>.Ignored,
            A<IReadOnlyCollection<KeyValuePair<FirstEntity, FirstEntityProperties>>>.Ignored,
            A<Scheme>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void MigratCollectionRemoveChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();
        var modelChanges = new ModelChanges();
        SetupModelChanges(modelChanges, CollectionAction.Remove, new(), null);

        storage.Migrate(_repository!, model, modelChanges);

        A.CallTo(() => _repository!.Delete(
            A<string>.Ignored,
            A<IReadOnlyCollection<FirstEntity>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void MigrateCollectionModifyChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();
        var modelChanges = new ModelChanges();
        SetupModelChanges(modelChanges, CollectionAction.Modify, new(), new());

        storage.Migrate(_repository!, model, modelChanges);

        A.CallTo(() => _repository!.Update(
            A<string>.Ignored,
            A<IReadOnlyCollection<KeyValuePair<FirstEntity, FirstEntityProperties>>>.Ignored,
            A<Scheme>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void MigrateRelationLinkChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();
        var modelChanges = new ModelChanges();
        SetupModelChanges(modelChanges, RelationAction.Linked);

        storage.Migrate(_repository!, model, modelChanges);

        A.CallTo(() => _repository!.Insert(
            A<string>.Ignored,
            A<IReadOnlyCollection<KeyValuePair<FirstEntity, SecondEntity>>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void MigratRelationUnlinkChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();
        var modelChanges = new ModelChanges();
        SetupModelChanges(modelChanges, RelationAction.Unlinked);

        storage.Migrate(_repository!, model, modelChanges);

        A.CallTo(() => _repository!.Delete(
            A<string>.Ignored,
            A<IReadOnlyCollection<KeyValuePair<FirstEntity, SecondEntity>>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveCollectionChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();
        var shard = A.Fake<IFakeModelShard>();

        storage.Save(_repository!, model);

        A.CallTo(() => _repository!.Insert(
            A<string>.Ignored,
            A<IReadOnlyCollection<KeyValuePair<FirstEntity, FirstEntityProperties>>>.Ignored,
            A<Scheme>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveRelationChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();
        var shard = A.Fake<IFakeModelShard>();

        storage.Save(_repository!, model);

        A.CallTo(() => _repository!.Insert(
            A<string>.Ignored,
            A<IReadOnlyCollection<KeyValuePair<FirstEntity, SecondEntity>>>.Ignored))
            .MustHaveHappened(4, Times.Exactly);
    }

    [Test]
    public void LoadCollectionChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();
        var shard = A.Fake<IFakeModelShard>();

        storage.Load(_repository!, model);

        A.CallTo(() => _repository!.Select(
            A<string>.Ignored,
            A<IMutableCollection<FirstEntity, FirstEntityProperties>>.Ignored,
            A<Scheme>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LoadRelationChangeTest()
    {
        var storage = new FakeModelShardStorage();
        var model = A.Fake<IModel>();
        var shard = A.Fake<IFakeModelShard>();

        storage.Load(_repository!, model);

        A.CallTo(() => _repository!.Select(
            A<string>.Ignored,
            A<IMutableRelation<FirstEntity, SecondEntity>>.Ignored,
            A<IEnumerable<FirstEntity>>.Ignored,
            A<IEnumerable<SecondEntity>>.Ignored))
            .MustHaveHappened(4, Times.Exactly);
    }

    private void SetupModelChanges(ModelChanges modelChanges, CollectionAction action, FirstEntityProperties? oldProps, FirstEntityProperties? newProps)
    {
        var changesFrame = A.Fake<IWritableChangesFrame>(c => c.Implements<IFakeChangesFrame>());
        A.CallTo(() => ((IFakeChangesFrame)changesFrame).FirstCollection.HasChanges()).Returns(true);
        A.CallTo(() => ((IFakeChangesFrame)changesFrame).FirstCollection.GetEnumerator())
            .Returns(new List<CollectionChange<FirstEntity, FirstEntityProperties>>()
            {
                new CollectionChange<FirstEntity, FirstEntityProperties>(action, new(), oldProps, newProps)
            }.GetEnumerator());
        modelChanges.Register(changesFrame);
    }

    private void SetupModelChanges(ModelChanges modelChanges, RelationAction action)
    {
        var changesFrame = A.Fake<IWritableChangesFrame>(c => c.Implements<IFakeChangesFrame>());
        A.CallTo(() => ((IFakeChangesFrame)changesFrame).OneToManyRelation.HasChanges()).Returns(true);
        A.CallTo(() => ((IFakeChangesFrame)changesFrame).OneToManyRelation.GetEnumerator())
            .Returns(new List<RelationChange<FirstEntity, SecondEntity>>()
            {
                new RelationChange<FirstEntity, SecondEntity>(action, new(), new())
            }.GetEnumerator());
        modelChanges.Register(changesFrame);
    }
}
