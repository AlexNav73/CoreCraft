using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.Commands;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;
using Navitski.Crystalized.Model.Tests.Infrastructure.Commands;

namespace Navitski.Crystalized.Model.Tests;

public class UndoRedoDomainModelTests
{
    [Test]
    public void HasChangesTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler, storage);
        var command = CreateCommand(model);

        command.Execute();

        Assert.That(model.HasChanges(), Is.True);
    }

    [Test]
    public void ChangedEventRaisedTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler, storage);
        var changedEventOccurred = false;
        model.Changed += (s, e) => changedEventOccurred = true;
        var command = CreateCommand(model);

        command.Execute();

        Assert.That(changedEventOccurred, Is.True);
    }

    private ModelCommand<UndoRedoDomainModel> CreateCommand(UndoRedoDomainModel model)
    {
        return new DelegateCommand<UndoRedoDomainModel>(model, m =>
        {
            var shard = m.Shard<IMutableFakeModelShard>();
            shard.FirstCollection.Add(new());
        });
    }
}
