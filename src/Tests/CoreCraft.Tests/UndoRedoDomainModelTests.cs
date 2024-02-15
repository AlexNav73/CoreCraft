using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Persistence;
using CoreCraft.Scheduling;
using CoreCraft.Subscription;

namespace CoreCraft.Tests;

public class UndoRedoDomainModelTests
{
    [Test]
    public void ConstructorDoesNotThrowTest()
    {
        var storage = A.Fake<IStorage>();
        Assert.DoesNotThrow(() => new UndoRedoDomainModel(new[] { new FakeModelShard() }));
    }

    [Test]
    [Ignore("In CI, worker thread has the same ID as test thread")]
    public async Task DefaultSchedulerIsAsyncTest()
    {
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() });
        var workerThread = -1;

        await model.Run<IMutableFakeModelShard>((shard, _) => workerThread = Environment.CurrentManagedThreadId);

        Assert.That(workerThread, Is.Not.EqualTo(-1));
        Assert.That(workerThread, Is.Not.EqualTo(Environment.CurrentManagedThreadId));
    }

    [Test]
    public async Task HasChangesTest()
    {
        var scheduler = new SyncScheduler();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler);

        await ExecuteAddCommand(model);

        Assert.That(model.HasChanges(), Is.True);
    }

    [Test]
    public async Task ChangedEventRaisedTest()
    {
        var scheduler = new SyncScheduler();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler);
        var changedEventOccurred = false;
        model.Changed += (s, e) => changedEventOccurred = true;

        await ExecuteAddCommand(model);

        Assert.That(changedEventOccurred, Is.True);
    }

    [Test]
    public async Task UndoStackHasOneChangeTest()
    {
        var scheduler = new SyncScheduler();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler);

        await ExecuteAddCommand(model);

        Assert.That(model.UndoStack.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task UndoStackIsEmptyAfterUndoExecutedTest()
    {
        var scheduler = new SyncScheduler();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler);

        await ExecuteAddCommand(model);

        await model.Undo();

        Assert.That(model.UndoStack.Count, Is.EqualTo(0));
        Assert.That(model.RedoStack.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task RedoStackIsEmptyAfterRedoExecutedTest()
    {
        var scheduler = new SyncScheduler();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler);
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
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler);

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
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler);
        var modelHasBeenChanged = false;

        model.Changed += (s, args) => modelHasBeenChanged = true;

        await ExecuteAddCommand(model);

        Assert.That(model.UndoStack.Count, Is.EqualTo(1));
        Assert.That(model.RedoStack.Count, Is.EqualTo(0));

        await model.Save(storage);

        A.CallTo(() => storage.Update(A<IEnumerable<IChangesFrame>>.Ignored))
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
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler);
        var modelHasBeenChanged = false;

        model.Changed += (s, args) => modelHasBeenChanged = true;

        await ExecuteAddCommand(model);

        Assert.That(model.UndoStack.Count, Is.EqualTo(1));
        Assert.That(model.RedoStack.Count, Is.EqualTo(0));

        await model.SaveAs(storage);

        A.CallTo(() => storage.Save(A<IEnumerable<IModelShard>>.Ignored)).MustHaveHappenedOnceExactly();

        Assert.That(model.UndoStack.Count, Is.EqualTo(0));
        Assert.That(model.RedoStack.Count, Is.EqualTo(0));
        Assert.That(modelHasBeenChanged, Is.True);
    }

    [Test]
    public async Task SaveAsWithOtherStorageUndoRedoDomainModelTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler);
        var modelHasBeenChanged = false;

        model.Changed += (s, args) => modelHasBeenChanged = true;

        await ExecuteAddCommand(model);

        Assert.That(model.UndoStack.Count, Is.EqualTo(1));
        Assert.That(model.RedoStack.Count, Is.EqualTo(0));

        await model.SaveAs(storage);

        A.CallTo(() => storage.Save(A<IEnumerable<IModelShard>>.Ignored)).MustHaveHappenedOnceExactly();

        Assert.That(model.UndoStack.Count, Is.EqualTo(0));
        Assert.That(model.RedoStack.Count, Is.EqualTo(0));
        Assert.That(modelHasBeenChanged, Is.True);
    }

    [Test]
    public async Task LoadCollectionUndoRedoDomainModelTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler);
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

            await model.Load(storage);

            A.CallTo(() => storage.Load(A<IEnumerable<IMutableModelShard>>.Ignored, A<bool>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.That(model.UndoStack.Count, Is.EqualTo(0), "Undo stack should be empty after model has been loaded. User should not see that model is changed, because it is loaded - not modified");
            Assert.That(model.RedoStack.Count, Is.EqualTo(0));
            Assert.That(firstCollectionChanged, Is.True);
        }
    }

    [Test]
    public async Task LoadRelationUndoRedoDomainModelTest()
    {
        var scheduler = new SyncScheduler();
        var storage = A.Fake<IStorage>();
        var model = new UndoRedoDomainModel(new[] { new FakeModelShard() }, scheduler);
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

            await model.Load(storage);

            A.CallTo(() => storage.Load(A<IEnumerable<IMutableModelShard>>.Ignored, A<bool>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.That(model.UndoStack.Count, Is.EqualTo(0), "Undo stack should be empty after model has been loaded. User should not see that model is changed, because it is loaded - not modified");
            Assert.That(model.RedoStack.Count, Is.EqualTo(0));
            Assert.That(relationChanged, Is.True);
        }
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
