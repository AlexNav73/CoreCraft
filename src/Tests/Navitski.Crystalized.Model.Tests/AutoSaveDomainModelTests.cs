using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;

namespace Navitski.Crystalized.Model.Tests;

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

        A.CallTo(() => storage.Update(path, A<IModel>.Ignored, A<IModelChanges>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task LoadAutoSaveDomainModelTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new AutoSaveDomainModel(new[] { new FakeModelShard() }, scheduler, storage, "fake");
        var firstCollectionChanged = false;

        model.SubscribeTo<IFakeChangesFrame>(x => x.With(y => y.FirstCollection).By(c => firstCollectionChanged = true));

        A.CallTo(() => storage.Load(A<string>.Ignored, A<IModel>.Ignored))
            .Invokes(c => c.Arguments.Get<IModel>(1)!.Shard<IMutableFakeModelShard>().FirstCollection.Add(new()));

        await model.Load();

        A.CallTo(() => storage.Load(A<string>.Ignored, A<IModel>.Ignored)).MustHaveHappenedOnceExactly();

        Assert.That(firstCollectionChanged, Is.True);
    }
}
