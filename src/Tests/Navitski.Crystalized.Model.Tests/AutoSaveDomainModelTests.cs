using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;
using Navitski.Crystalized.Model.Tests.Infrastructure.Commands;

namespace Navitski.Crystalized.Model.Tests;

public class AutoSaveDomainModelTests
{
    [Test]
    public void SaveIsCalledAutomaticallyAfterChangesHappenedTest()
    {
        var path = "test";
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new AutoSaveDomainModel(new[] { new FakeModelShard() }, scheduler, storage, path);
        var command = new DelegateCommand<AutoSaveDomainModel>(model, m =>
        {
            var shard = m.Shard<IMutableFakeModelShard>();
            shard.FirstCollection.Add(new());
        });

        command.Execute();

        A.CallTo(() => storage.Migrate(path, A<IModel>.Ignored, A<IEnumerable<IModelChanges>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
