using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;
using Navitski.Crystalized.Model.Engine.Subscription.Builders;

namespace Navitski.Crystalized.Model.Tests.Engine;

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
    public async Task RedoStackIsEmptyAfterRedoExecutedTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler, storage);
        var modelHasBeenChanged = false;

        await ExecuteAddCommand(model);
        await model.Undo();

        model.Changed += (s, a) => modelHasBeenChanged = true;

        Assert.That(model.UndoStack.Count, Is.EqualTo(0));
        Assert.That(model.RedoStack.Count, Is.EqualTo(1));

        await model.Redo();

        Assert.That(model.UndoStack.Count, Is.EqualTo(1));
        Assert.That(model.RedoStack.Count, Is.EqualTo(0));
        Assert.That(modelHasBeenChanged, Is.True);
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

    [Test]
    public async Task SaveUndoRedoDomainModelTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler, storage);
        var modelHasBeenChanged = false;

        model.Changed += (s, args) => modelHasBeenChanged = true;

        await ExecuteAddCommand(model);

        Assert.That(model.UndoStack.Count, Is.EqualTo(1));
        Assert.That(model.RedoStack.Count, Is.EqualTo(0));

        await model.Save("fake");

        A.CallTo(() => storage.Update(A<string>.Ignored, A<IModelChanges>.Ignored))
            .MustHaveHappenedOnceExactly();

        Assert.That(model.UndoStack.Count, Is.EqualTo(0));
        Assert.That(model.RedoStack.Count, Is.EqualTo(0));
        Assert.That(modelHasBeenChanged, Is.True);
    }

    [Test]
    public async Task SaveAsUndoRedoDomainModelTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler, storage);
        var modelHasBeenChanged = false;

        model.Changed += (s, args) => modelHasBeenChanged = true;

        await ExecuteAddCommand(model);

        Assert.That(model.UndoStack.Count, Is.EqualTo(1));
        Assert.That(model.RedoStack.Count, Is.EqualTo(0));

        await model.SaveAs("fake");

        A.CallTo(() => storage.Save(A<string>.Ignored, A<IModel>.Ignored)).MustHaveHappenedOnceExactly();

        Assert.That(model.UndoStack.Count, Is.EqualTo(0));
        Assert.That(model.RedoStack.Count, Is.EqualTo(0));
        Assert.That(modelHasBeenChanged, Is.True);
    }

    [Test]
    public async Task SaveAsWithOtherStorageUndoRedoDomainModelTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var storage2 = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler, storage);
        var modelHasBeenChanged = false;

        model.Changed += (s, args) => modelHasBeenChanged = true;

        await ExecuteAddCommand(model);

        Assert.That(model.UndoStack.Count, Is.EqualTo(1));
        Assert.That(model.RedoStack.Count, Is.EqualTo(0));

        await model.SaveAs("fake", storage2);

        A.CallTo(() => storage2.Save(A<string>.Ignored, A<IModel>.Ignored)).MustHaveHappenedOnceExactly();

        Assert.That(model.UndoStack.Count, Is.EqualTo(0));
        Assert.That(model.RedoStack.Count, Is.EqualTo(0));
        Assert.That(modelHasBeenChanged, Is.True);
    }

    [Test]
    public async Task LoadCollectionUndoRedoDomainModelTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler, storage);
        var firstCollectionChanged = false;

        model.For<IFakeChangesFrame>().With(y => y.FirstCollection).Subscribe(c => firstCollectionChanged = true);

        A.CallTo(() => storage.Load(A<string>.Ignored, A<IModel>.Ignored))
            .Invokes(c => c.Arguments.Get<IModel>(1)!.Shard<IMutableFakeModelShard>().FirstCollection.Add(new()));

        await model.Load("fake");

        A.CallTo(() => storage.Load(A<string>.Ignored, A<IModel>.Ignored)).MustHaveHappenedOnceExactly();

        Assert.That(model.UndoStack.Count, Is.EqualTo(0), "Undo stack should be empty after model has been loaded. User should not see that model is changed, because it is loaded - not modified");
        Assert.That(model.RedoStack.Count, Is.EqualTo(0));
        Assert.That(firstCollectionChanged, Is.True);
    }

    [Test]
    public async Task LoadRelationUndoRedoDomainModelTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler, storage);
        var relationChanged = false;

        model.For<IFakeChangesFrame>().With(y => y.OneToOneRelation).Subscribe(c => relationChanged = true);

        A.CallTo(() => storage.Load(A<string>.Ignored, A<IModel>.Ignored))
            .Invokes(c => c.Arguments.Get<IModel>(1)!.Shard<IMutableFakeModelShard>().OneToOneRelation.Add(new(), new()));

        await model.Load("fake");

        A.CallTo(() => storage.Load(A<string>.Ignored, A<IModel>.Ignored)).MustHaveHappenedOnceExactly();

        Assert.That(model.UndoStack.Count, Is.EqualTo(0), "Undo stack should be empty after model has been loaded. User should not see that model is changed, because it is loaded - not modified");
        Assert.That(model.RedoStack.Count, Is.EqualTo(0));
        Assert.That(relationChanged, Is.True);
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
