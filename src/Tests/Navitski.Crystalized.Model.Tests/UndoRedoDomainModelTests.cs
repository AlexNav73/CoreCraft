using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;

namespace Navitski.Crystalized.Model.Tests;

public class UndoRedoDomainModelTests
{
    [Test]
    public async Task HasChangesTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler, storage);

        await ExecuteAddCommand(model);

        Assert.That(model.HasChanges(), Is.True);
    }

    [Test]
    public async Task ChangedEventRaisedTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler, storage);
        var changedEventOccurred = false;
        model.Changed += (s, e) => changedEventOccurred = true;

        await ExecuteAddCommand(model);

        Assert.That(changedEventOccurred, Is.True);
    }

    [Test]
    public async Task UndoStackHasOneChangeTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler, storage);

        await ExecuteAddCommand(model);

        Assert.That(model.UndoStack.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task UndoStackIsEmptyAfterUndoExecutedTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler, storage);

        await ExecuteAddCommand(model);

        await model.Undo();

        Assert.That(model.UndoStack.Count, Is.EqualTo(0));
        Assert.That(model.RedoStack.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task RedoStackMustBeDroppedWhenNewChangesHappenedTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler, storage);

        await ExecuteAddCommand(model);

        var entity = model.Shard<IFakeModelShard>().FirstCollection.Single();

        await ExecuteModifyCommand(model, entity, "test");

        model.Undo().GetAwaiter().GetResult();

        await ExecuteRemoveCommand(model, entity);

        Assert.DoesNotThrow(() => model.Redo().GetAwaiter().GetResult());
    }

    private static async Task ExecuteAddCommand(IDomainModel model)
    {
        await model.Run<IMutableFakeModelShard>((shard, _) => shard.FirstCollection.Add(new()));
    }

    private static async Task ExecuteRemoveCommand(IDomainModel model, FirstEntity entity)
    {
        await model.Run<IMutableFakeModelShard>((shard, _) => shard.FirstCollection.Remove(entity));
    }

    private static async Task ExecuteModifyCommand(IDomainModel model, FirstEntity entity, string value)
    {
        await model.Run<IMutableFakeModelShard>((shard, _) => shard.FirstCollection.Modify(entity, p => p with { NullableStringProperty = value }));
    }
}
