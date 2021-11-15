using PricingCalc.Model.Engine.ChangesTracking;
using PricingCalc.Model.Engine.Commands;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.Engine;

public abstract class BaseModel : IBaseModel, ICommandRunner
{
    private readonly View _view;
    private readonly IJobService _jobService;
    private readonly HashSet<Action<ModelChangedEventArgs>> _subscriptions;

    private volatile ModelChangedEventArgs? _currentChanges;

    protected BaseModel(IEnumerable<IModelShard> shards, IJobService jobService)
    {
        _subscriptions = new HashSet<Action<ModelChangedEventArgs>>();
        _view = new View(shards);
        _jobService = jobService;
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

    async Task ICommandRunner.Run(IRunnable runnable)
    {
        var snapshot = _view.CreateTrackableSnapshot();

        try
        {
            await _jobService.Enqueue(() => runnable.Run(snapshot));
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Command execution failed. Command {Command}", runnable.GetType());

            throw;
        }

        var result = _view.ApplySnapshot(snapshot, snapshot.Changes);
        if (result.Changes.HasChanges())
        {
            NotifySubscribers(result);
            RaiseEvent(result);
        }
    }

    public async Task Save(IStorage storage, string path, IReadOnlyList<IModelChanges> changes)
    {
        // Create disconnected copy of model to send it for saving
        // If some changes would be made to the model while saving,
        // it wouldn't break saving, because we will save copy of the model
        // created when user pressed the Save button
        var copy = _view.CopyModel();

        await _jobService.RunParallel(() => storage.Save(path, copy, changes));
    }

    public async Task Save(IStorage storage, string path)
    {
        // Create disconnected copy of model to send it for saving
        // If some changes would be made to the model while saving,
        // it wouldn't break saving, because we will save copy of the model
        // created when user pressed the Save button
        var copy = _view.CopyModel();

        await _jobService.RunParallel(() => storage.Save(path, copy));
    }

    public async Task Load(IStorage storage, string path)
    {
        var snapshot = _view.CreateTrackableSnapshot();

        try
        {
            await _jobService.Enqueue(() => storage.Load(path, snapshot));
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Model loading failed");

            throw;
        }

        var result = _view.ApplySnapshot(snapshot, snapshot.Changes);
        if (result.Changes.HasChanges())
        {
            NotifySubscribers(result);
        }
    }

    public async Task Apply(IWritableModelChanges changes)
    {
        if (changes.HasChanges())
        {
            var snapshot = _view.CreateSnapshot();

            try
            {
                await _jobService.Enqueue(() => changes.Apply(snapshot));
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Applying changes has failed");

                throw;
            }

            var result = _view.ApplySnapshot(snapshot, changes);
            NotifySubscribers(result);
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
