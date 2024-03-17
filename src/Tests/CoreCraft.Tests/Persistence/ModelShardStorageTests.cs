using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Features.CoW;
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
    public void UpdateCollectionTest()
    {
        var shard = new FakeChangesFrame();

        shard.Update(_repository!);

        A.CallTo(() => _repository!.Update(
            A<ICollectionChangeSet<FirstEntity, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void UpdateRelationTest()
    {
        var shard = new FakeChangesFrame();

        shard.Update(_repository!);

        A.CallTo(() => _repository!.Update(
            A<IRelationChangeSet<FirstEntity, SecondEntity>>.Ignored))
            .MustHaveHappened(4, Times.Exactly);
    }

    [Test]
    public void SaveCollectionTest()
    {
        var shard = new FakeModelShard();

        shard.Save(_repository!);

        A.CallTo(() => _repository!.Save(
            A<ICollection<FirstEntity, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SaveRelationTest()
    {
        var shard = new FakeModelShard();

        shard.Save(_repository!);

        A.CallTo(() => _repository!.Save(
            A<IRelation<FirstEntity, SecondEntity>>.Ignored))
            .MustHaveHappened(4, Times.Exactly);
    }

    [Test]
    public void LoadCollectionTest()
    {
        var shard = new FakeModelShard().AsMutable([new CoWFeature()]);

        shard.Load(_repository!);

        A.CallTo(() => _repository!.Load(
            A<IMutableCollection<FirstEntity, FirstEntityProperties>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void LoadRelationTest()
    {
        var shard = new FakeModelShard().AsMutable([new CoWFeature()]);

        shard.Load(_repository!);

        A.CallTo(() => _repository!.Load(
            A<IMutableRelation<FirstEntity, SecondEntity>>.Ignored,
            A<IEnumerable<FirstEntity>>.Ignored,
            A<IEnumerable<SecondEntity>>.Ignored))
            .MustHaveHappened(4, Times.Exactly);
    }
}
