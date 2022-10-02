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
        var command = CreateAddCommand(model);

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
        var command = CreateAddCommand(model);

        command.Execute();

        Assert.That(changedEventOccurred, Is.True);
    }

    [Test]
    public void RedoStackMustBeDroppedWhenNewChangesHappenedTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler, storage);
        var addCommand = CreateAddCommand(model);

        addCommand.Execute();

        var entity = model.Shard<IFakeModelShard>().FirstCollection.Single();
        var removeCommand = CreateRemoveCommand(model, entity);
        var modifyCommand = CreateModifyCommand(model, entity, "test");

        modifyCommand.Execute();

        model.Undo().GetAwaiter().GetResult();

        removeCommand.Execute();

        Assert.DoesNotThrow(() => model.Redo().GetAwaiter().GetResult());
    }

    private ModelCommand<UndoRedoDomainModel> CreateAddCommand(UndoRedoDomainModel model)
    {
        return new DelegateCommand<UndoRedoDomainModel>(model, m =>
        {
            var shard = m.Shard<IMutableFakeModelShard>();
            shard.FirstCollection.Add(new());
        });
    }

    private ModelCommand<UndoRedoDomainModel> CreateRemoveCommand(UndoRedoDomainModel model, FirstEntity entity)
    {
        return new DelegateCommand<UndoRedoDomainModel>(model, m =>
        {
            var shard = m.Shard<IMutableFakeModelShard>();
            shard.FirstCollection.Remove(entity);
        });
    }

    private ModelCommand<UndoRedoDomainModel> CreateModifyCommand(UndoRedoDomainModel model, FirstEntity entity, string value)
    {
        return new DelegateCommand<UndoRedoDomainModel>(model, m =>
        {
            var shard = m.Shard<IMutableFakeModelShard>();
            shard.FirstCollection.Modify(entity, p => p with { NullableStringProperty = value });
        });
    }
}
