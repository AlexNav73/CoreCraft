using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Persistence;
using CoreCraft.Persistence.History;
using CoreCraft.Scheduling;
using CoreCraft.Subscription.Extensions;

namespace CoreCraft.Tests;

public class UndoRedoDomainModelTests
{
    private UndoRedoDomainModel _model;

    [SetUp]
    public void Setup()
    {
        _model = new UndoRedoDomainModel(new[] { new FakeModelShard() });
    }

    [Test]
    public void ConstructorDoesNotThrowTest()
    {
        var storage = A.Fake<IStorage>();
        Assert.DoesNotThrow(() => new UndoRedoDomainModel(new[] { new FakeModelShard() }));
    }

    [Test]
    public void DefaultSchedulerIsAsyncTest()
    {
        Assert.That(_model.Scheduler, Is.TypeOf<AsyncScheduler>());
    }

    [Test]
    public async Task HasChangesUndoRedoDomainModelTest()
    {
        await ExecuteAddCommand();

        Assert.That(_model.History.HasChanges(), Is.True);
    }

    [Test]
    public async Task ChangedEventRaisedTest()
    {
        var changedEventOccurred = false;
        _model.History.Changed += (s, e) => changedEventOccurred = true;

        await ExecuteAddCommand();

        Assert.That(changedEventOccurred, Is.True);
    }

    [Test]
    public async Task UndoStackHasOneChangeTest()
    {
        await ExecuteAddCommand();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task UndoStackIsEmptyAfterUndoExecutedTest()
    {
        await ExecuteAddCommand();

        await _model.History.Undo();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(0));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task RedoStackIsEmptyAfterRedoExecutedTest()
    {
        var modelHasBeenChanged = false;

        await ExecuteAddCommand();
        await _model.History.Undo();

        _model.History.Changed += (s, a) => modelHasBeenChanged = true;

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(0));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(1));

        await _model.History.Redo();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(1));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));
        Assert.That(modelHasBeenChanged, Is.True);
    }

    [Test]
    public async Task RedoStackMustBeDroppedWhenNewChangesHappenedTest()
    {
        await ExecuteAddCommand();

        var entity = _model.Shard<IFakeModelShard>().FirstCollection.Single();

        await ExecuteModifyCommand(entity, "test");

        await _model.History.Undo();

        await ExecuteRemoveCommand(entity);

        Assert.DoesNotThrowAsync(_model.History.Redo);
    }

    [Test]
    public async Task UpdateUndoRedoDomainModelTest()
    {
        var storage = A.Fake<IStorage>();
        var modelHasBeenChanged = false;

        _model.History.Changed += (s, args) => modelHasBeenChanged = true;

        await ExecuteAddCommand();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(1));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));

        await _model.History.Update(storage);

        A.CallTo(() => storage.Update(A<IEnumerable<IChangesFrame>>.Ignored))
            .MustHaveHappenedOnceExactly();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(0));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));
        Assert.That(modelHasBeenChanged, Is.True);
    }

    [Test]
    public async Task SaveAsUndoRedoDomainModelTest()
    {
        var storage = A.Fake<IStorage>();
        var modelHasBeenChanged = false;

        _model.History.Changed += (s, args) => modelHasBeenChanged = true;

        await ExecuteAddCommand();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(1));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));

        await _model.Save(storage);

        A.CallTo(() => storage.Save(A<IEnumerable<IModelShard>>.Ignored)).MustHaveHappenedOnceExactly();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(1));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));
        Assert.That(modelHasBeenChanged, Is.True);
    }

    [Test]
    public async Task SaveAsWithOtherStorageUndoRedoDomainModelTest()
    {
        var storage = A.Fake<IStorage>();
        var modelHasBeenChanged = false;

        _model.History.Changed += (s, args) => modelHasBeenChanged = true;

        await ExecuteAddCommand();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(1));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));

        await _model.Save(storage);

        A.CallTo(() => storage.Save(A<IEnumerable<IModelShard>>.Ignored)).MustHaveHappenedOnceExactly();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(1));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));
        Assert.That(modelHasBeenChanged, Is.True);
    }

    [Test]
    public async Task SaveHistoryUndoRedoDomainModelTest()
    {
        var storage = A.Fake<IHistoryStorage>();

        await ExecuteAddCommand();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(1));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));

        await _model.History.Save(storage);

        A.CallTo(() => storage.Save(A<IEnumerable<IModelChanges>>.Ignored))
            .MustHaveHappenedOnceExactly();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(0));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task ClearHistoryUndoRedoDomainModelTest()
    {
        await ExecuteAddCommand();
        await ExecuteAddCommand();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(2));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));

        await _model.History.Undo();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(1));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(1));

        _model.History.Clear();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(0));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task LoadCollectionUndoRedoDomainModelTest()
    {
        var storage = A.Fake<IStorage>();
        var firstCollectionChanged = false;

        using (_model.For<IFakeChangesFrame>().With(y => y.FirstCollection).Subscribe(c => firstCollectionChanged = true))
        {
            A.CallTo(() => storage.Load(A<IEnumerable<IMutableModelShard>>.Ignored, A<bool>.Ignored))
                .Invokes(c =>
                {
                    var loadables = c.Arguments.Get<IEnumerable<IMutableModelShard>>(0)!;
                    var shard = loadables.OfType<IMutableFakeModelShard>().Single();

                    shard.FirstCollection.Add(new());
                });

            await _model.Load(storage);

            A.CallTo(() => storage.Load(A<IEnumerable<IMutableModelShard>>.Ignored, A<bool>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.That(_model.History.UndoStack.Count, Is.EqualTo(0), "Undo stack should be empty after model has been loaded. User should not see that model is changed, because it is loaded - not modified");
            Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));
            Assert.That(firstCollectionChanged, Is.True);
        }
    }

    [Test]
    public async Task LoadRelationUndoRedoDomainModelTest()
    {
        var storage = A.Fake<IStorage>();
        var relationChanged = false;

        using (_model.For<IFakeChangesFrame>().With(y => y.OneToOneRelation).Subscribe(c => relationChanged = true))
        {
            A.CallTo(() => storage.Load(A<IEnumerable<IMutableModelShard>>.Ignored, A<bool>.Ignored))
                .Invokes(c =>
                {
                    var loadables = c.Arguments.Get<IEnumerable<IMutableModelShard>>(0)!;
                    var shard = loadables.OfType<IMutableFakeModelShard>().Single();

                    shard.OneToOneRelation.Add(new(), new());
                });

            await _model.Load(storage);

            A.CallTo(() => storage.Load(A<IEnumerable<IMutableModelShard>>.Ignored, A<bool>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.That(_model.History.UndoStack.Count, Is.EqualTo(0), "Undo stack should be empty after model has been loaded. User should not see that model is changed, because it is loaded - not modified");
            Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));
            Assert.That(relationChanged, Is.True);
        }
    }

    [Test]
    public async Task LoadHistoryUndoRedoDomainModelTest()
    {
        var storage = A.Fake<IHistoryStorage>();

        A.CallTo(() => storage.Load(A<IEnumerable<IModelShard>>.Ignored))
            .Returns([new ModelChanges(0)]);

        await _model.History.Load(storage);

        A.CallTo(() => storage.Load(A<IEnumerable<IModelShard>>.Ignored))
            .MustHaveHappenedOnceExactly();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(1), "Undo stack should be empty after model has been loaded. User should not see that model is changed, because it is loaded - not modified");
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task RestoreHistoryShouldDoNothingWhenModelHistoryIsNotEmptyTest()
    {
        var storage = A.Fake<IHistoryStorage>();

        A.CallTo(() => storage.Load(A<IEnumerable<IModelShard>>.Ignored))
            .Returns([new ModelChanges(0)]);

        await ExecuteAddCommand();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(1));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));

        var change = _model.History.UndoStack.Single();

        await _model.History.Load(storage);

        A.CallTo(() => storage.Load(A<IEnumerable<IModelShard>>.Ignored))
            .MustNotHaveHappened();

        Assert.That(_model.History.UndoStack.Count, Is.EqualTo(1));
        Assert.That(_model.History.RedoStack.Count, Is.EqualTo(0));
        Assert.That(ReferenceEquals(change, _model.History.UndoStack.Single()), Is.True);
    }

    private async Task ExecuteAddCommand()
    {
        await _model.Run<IMutableFakeModelShard>(static (shard, _) => shard.FirstCollection.Add(new()));
    }

    private async Task ExecuteRemoveCommand(FirstEntity entity)
    {
        await _model.Run<IMutableFakeModelShard>((shard, _) => shard.FirstCollection.Remove(entity));
    }

    private async Task ExecuteModifyCommand(FirstEntity entity, string value)
    {
        await _model.Run<IMutableFakeModelShard>((shard, _) => shard.FirstCollection.Modify(entity, p => p with { NullableStringProperty = value }));
    }
}
