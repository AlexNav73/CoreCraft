using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Commands;
using Navitski.Crystalized.Model.Engine.Exceptions;
using Navitski.Crystalized.Model.Engine.Persistence;

namespace Navitski.Crystalized.Model.Engine;

public abstract class BaseModel : IBaseModel, ICommandRunner
{
    private readonly View _view;
    private readonly IScheduler _scheduler;
    private readonly HashSet<Action<ModelChangedEventArgs>> _subscriptions;

    private volatile ModelChangedEventArgs? _currentChanges;

    protected BaseModel(IEnumerable<IModelShard> shards, ModelConfiguration configuration)
    {
        _subscriptions = new HashSet<Action<ModelChangedEventArgs>>();
        _view = new View(shards);
        _scheduler = configuration.Scheduler;
    }

    public event EventHandler<ModelChangedEventArgs>? ModelChanged;

    public T Shard<T>() where T : IModelShard
    {
        return _view.UnsafeModel.Shard<T>();
    }

    public IDisposable Subscribe(Action<ModelChangedEventArgs> onModelChanges)
    {
        if (!_subscriptions.Contains(onModelChanges))
        {
            _subscriptions.Add(onModelChanges);
        }

        if (_currentChanges != null)
        {
            onModelChanges(_currentChanges);
        }

        return new UnsubscribeOnDispose(onModelChanges, _subscriptions);
    }

    public async Task Save(IStorage storage, string path, IReadOnlyList<IModelChanges> changes, CancellationToken token = default)
    {
        var copy = _view.CopyModel();

        await _scheduler.RunParallel(() => storage.Save(path, copy, changes), token);
    }

    public async Task Save(IStorage storage, string path, CancellationToken token = default)
    {
        var copy = _view.CopyModel();

        await _scheduler.RunParallel(() => storage.Save(path, copy), token);
    }

    public async Task Load(IStorage storage, string path, CancellationToken token = default)
    {
        var snapshot = _view.CreateTrackableSnapshot();

        try
        {
            await _scheduler.Enqueue(() => storage.Load(path, snapshot), token);
        }
        catch (Exception ex)
        {
            throw new ModelLoadingException("Model loading failed", ex);
        }

        var result = _view.ApplySnapshot(snapshot, snapshot.Changes);
        if (result.Changes.HasChanges())
        {
            NotifySubscribers(result);
        }
    }

    public async Task Apply(IWritableModelChanges changes, CancellationToken token = default)
    {
        if (changes.HasChanges())
        {
            var snapshot = _view.CreateSnapshot();

            try
            {
                await _scheduler.Enqueue(() => changes.Apply(snapshot), token);
            }
            catch (Exception ex)
            {
                throw new ApplyModelChangesException("Applying changes has failed", ex);
            }

            var result = _view.ApplySnapshot(snapshot, changes);
            NotifySubscribers(result);
        }
    }

    async Task ICommandRunner.Enqueue(IRunnable runnable, CancellationToken token)
    {
        var snapshot = _view.CreateTrackableSnapshot();

        try
        {
            await _scheduler.Enqueue(() => runnable.Run(snapshot), token);
        }
        catch (Exception ex)
        {
            throw new CommandInvokationException($"Command execution failed. Command {runnable.GetType()}", ex);
        }

        var result = _view.ApplySnapshot(snapshot, snapshot.Changes);
        if (result.Changes.HasChanges())
        {
            NotifySubscribers(result);
            RaiseEvent(result);
        }
    }

    private void NotifySubscribers(ModelChangeResult result)
    {
        _currentChanges = new ModelChangedEventArgs(result.OldModel, result.NewModel, result.Changes);

        var observers = _subscriptions.ToArray();
        foreach (var observer in observers)
        {
            observer(_currentChanges);
        }

        _currentChanges = null;
    }

    private void RaiseEvent(ModelChangeResult result)
    {
        ModelChanged?.Invoke(this, new ModelChangedEventArgs(result.OldModel, result.NewModel, result.Changes));
    }
}
