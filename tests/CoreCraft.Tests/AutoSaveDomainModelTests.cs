using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Persistence;
using CoreCraft.Scheduling;
using CoreCraft.Subscription;

namespace CoreCraft.Tests;

public class AutoSaveDomainModelTests
{
    [Test]
    public async Task SaveIsCalledAutomaticallyAfterChangesHappenedTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new AutoSaveDomainModel(new[] { new FakeModelShard() }, scheduler, storage);

        await model.Run<IMutableFakeModelShard>((shard, _) => shard.FirstCollection.Add(new()));

        A.CallTo(() => storage.Update(A<IEnumerable<IChangesFrame>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task LoadCollectionAutoSaveDomainModelTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new AutoSaveDomainModel(new[] { new FakeModelShard() }, scheduler, storage);
        var firstCollectionChanged = false;

        using (model.For<IFakeChangesFrame>().With(y => y.FirstCollection).Subscribe(c => firstCollectionChanged = true))
        {
            A.CallTo(() => storage.Load(A<IEnumerable<IMutableModelShard>>.Ignored, A<bool>.Ignored))
                .Invokes(c =>
                {
                    var loadables = c.Arguments.Get<IEnumerable<IMutableModelShard>>(0)!;
                    var shard = loadables.OfType<IMutableFakeModelShard>().Single();

                    shard.FirstCollection.Add(new());
                });

            await model.Load();

            A.CallTo(() => storage.Load(A<IEnumerable<IMutableModelShard>>.Ignored, A<bool>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.That(firstCollectionChanged, Is.True);
        }
    }

    [Test]
    public async Task LoadRelationAutoSaveDomainModelTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new AutoSaveDomainModel(new[] { new FakeModelShard() }, scheduler, storage);
        var relationChanged = false;

        using (model.For<IFakeChangesFrame>().With(y => y.OneToOneRelation).Subscribe(c => relationChanged = true))
        {
            A.CallTo(() => storage.Load(A<IEnumerable<IMutableModelShard>>.Ignored, A<bool>.Ignored))
                .Invokes(c =>
                {
                    var loadables = c.Arguments.Get<IEnumerable<IMutableModelShard>>(0)!;
                    var shard = loadables.OfType<IMutableFakeModelShard>().Single();

                    shard.OneToOneRelation.Add(new(), new());
                });

            await model.Load();

            A.CallTo(() => storage.Load(A<IEnumerable<IMutableModelShard>>.Ignored, A<bool>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.That(relationChanged, Is.True);
        }
    }
}
