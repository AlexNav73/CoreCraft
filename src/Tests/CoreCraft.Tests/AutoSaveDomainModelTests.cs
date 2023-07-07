using CoreCraft;
using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Persistence;
using CoreCraft.Scheduling;
using CoreCraft.Subscription.Builders;

namespace CoreCraft.Tests;

public class AutoSaveDomainModelTests
{
    [Test]
    public async Task SaveIsCalledAutomaticallyAfterChangesHappenedTest()
    {
        var path = "test";
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new AutoSaveDomainModel(new[] { new FakeModelShard() }, scheduler, storage, path);

        await model.Run<IMutableFakeModelShard>((shard, _) => shard.FirstCollection.Add(new()));

        A.CallTo(() => storage.Update(path, A<IModelChanges>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task LoadCollectionAutoSaveDomainModelTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new AutoSaveDomainModel(new[] { new FakeModelShard() }, scheduler, storage, "fake");
        var firstCollectionChanged = false;

        model.For<IFakeChangesFrame>().With(y => y.FirstCollection).Subscribe(c => firstCollectionChanged = true);

        A.CallTo(() => storage.Load(A<string>.Ignored, A<IModel>.Ignored))
            .Invokes(c => c.Arguments.Get<IModel>(1)!.Shard<IMutableFakeModelShard>().FirstCollection.Add(new()));

        await model.Load();

        A.CallTo(() => storage.Load(A<string>.Ignored, A<IModel>.Ignored)).MustHaveHappenedOnceExactly();

        Assert.That(firstCollectionChanged, Is.True);
    }

    [Test]
    public async Task LoadRelationAutoSaveDomainModelTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new AutoSaveDomainModel(new[] { new FakeModelShard() }, scheduler, storage, "fake");
        var relationChanged = false;

        model.For<IFakeChangesFrame>().With(y => y.OneToOneRelation).Subscribe(c => relationChanged = true);

        A.CallTo(() => storage.Load(A<string>.Ignored, A<IModel>.Ignored))
            .Invokes(c => c.Arguments.Get<IModel>(1)!.Shard<IMutableFakeModelShard>().OneToOneRelation.Add(new(), new()));

        await model.Load();

        A.CallTo(() => storage.Load(A<string>.Ignored, A<IModel>.Ignored)).MustHaveHappenedOnceExactly();

        Assert.That(relationChanged, Is.True);
    }
}
