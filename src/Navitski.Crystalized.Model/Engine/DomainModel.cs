using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Commands;
using Navitski.Crystalized.Model.Engine.Exceptions;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;

namespace Navitski.Crystalized.Model.Engine;

/// <summary>
///     A base class for model implementation
/// </summary>
public abstract class DomainModel : IDomainModel, ICommandRunner
{
    private readonly View _view;
    private readonly IScheduler _scheduler;
    private readonly HashSet<Action<ModelChangedEventArgs>> _subscriptions;

    private volatile ModelChangedEventArgs? _currentChanges;

    /// <summary>
    ///     Ctor
    /// </summary>
    protected DomainModel(IEnumerable<IModelShard> shards, IScheduler scheduler)
    {
        _subscriptions = new HashSet<Action<ModelChangedEventArgs>>();
        _view = new View(shards);
        _scheduler = scheduler;
    }

    /// <summary>
    ///     Raised after all subscribers handled changes
    /// </summary>
    protected virtual void OnModelChanged(ModelChangedEventArgs args)
    {
    }

    /// <inheritdoc cref="IModelShardAccessor.Shard{T}"/>
    public T Shard<T>() where T : IModelShard
    {
        return _view.UnsafeModel.Shard<T>();
    }

    /// <inheritdoc cref="IDomainModel.Subscribe(Action{ModelChangedEventArgs})"/>
    public IDisposable Subscribe(Action<ModelChangedEventArgs> onModelChanges)
    {
        if (_subscriptions.Contains(onModelChanges))
        {
            throw new SubscriptionAlreadyExistsException("Subscription already exists");
        }

        _subscriptions.Add(onModelChanges);
        if (_currentChanges != null)
        {
            onModelChanges(_currentChanges);
        }

        return new UnsubscribeOnDispose(onModelChanges, _subscriptions);
    }

    /// <summary>
    ///     Saves a list of model changes
    /// </summary>
    /// <param name="storage">A storage to write</param>
    /// <param name="path">A path of a file</param>
    /// <param name="changes">A list of changes</param>
    /// <param name="token">Cancellation token</param>
    /// <exception cref="ModelSaveException">Throws when an error occurred while saving the model</exception>
    protected Task Save(IStorage storage, string path, IReadOnlyList<IModelChanges> changes, CancellationToken token = default)
    {
        var copy = _view.CopyModel();

        try
        {
            return _scheduler.RunParallel(() => storage.Migrate(path, copy, changes), token);
        }
        catch (Exception ex)
        {
            throw new ModelSaveException("Model save has failed", ex);
        }
    }

    /// <summary>
    ///     Saves a model as a whole when storage is empty
    /// </summary>
    /// <param name="storage">A storage</param>
    /// <param name="path">A path to a file</param>
    /// <param name="token">Cancellation token</param>
    /// <exception cref="ModelSaveException">Throws when an error occurred while saving the model</exception>
    protected Task Save(IStorage storage, string path, CancellationToken token = default)
    {
        var copy = _view.CopyModel();

        try
        {
            return _scheduler.RunParallel(() => storage.Save(path, copy), token);
        }
        catch (Exception ex)
        {
            throw new ModelSaveException("Model save has failed", ex);
        }
    }

    /// <summary>
    ///     Loads a model
    /// </summary>
    /// <param name="storage">A storage</param>
    /// <param name="path">A path to a file</param>
    /// <param name="token">Cancellation token</param>
    /// <exception cref="ModelLoadingException">Throws when an error occurred while loading the model</exception>
    protected async Task Load(IStorage storage, string path, CancellationToken token = default)
    {
        var snapshot = _view.CreateTrackableSnapshot();

        try
        {
            await _scheduler.Enqueue(() => storage.Load(path, snapshot), token);
        }
        catch (Exception ex)
        {
            throw new ModelLoadingException("Model loading has failed", ex);
        }

        var result = _view.ApplySnapshot(snapshot, snapshot.Changes);
        if (result.Changes.HasChanges())
        {
            var eventArgs = CreateModelChangesEventArgs(result);

            NotifySubscribers(eventArgs);
        }
    }

    /// <summary>
    ///     Applies changes to the model
    /// </summary>
    /// <param name="changes">Changes to apply</param>
    /// <param name="token">Cancellation token</param>
    /// <exception cref="ApplyModelChangesException">Throws when an error occurred while applying changes to the model</exception>
    protected async Task Apply(IWritableModelChanges changes, CancellationToken token = default)
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
            var eventArgs = CreateModelChangesEventArgs(result);

            NotifySubscribers(eventArgs);
        }
    }

    async Task ICommandRunner.Enqueue(IRunnable runnable, CancellationToken token)
    {
        var snapshot = _view.CreateTrackableSnapshot();

        try
        {
            await _scheduler.Enqueue(() => runnable.Run(snapshot, token), token);
        }
        catch (Exception ex)
        {
            throw new CommandInvokationException($"Command execution failed. Command {runnable.GetType()}", ex);
        }

        var result = _view.ApplySnapshot(snapshot, snapshot.Changes);
        if (result.Changes.HasChanges())
        {
            var eventArgs = CreateModelChangesEventArgs(result);
            
            NotifySubscribers(eventArgs);
            OnModelChanged(eventArgs);
        }
    }

    private void NotifySubscribers(ModelChangedEventArgs eventArgs)
    {
        _currentChanges = eventArgs;

        var observers = _subscriptions.ToArray();
        foreach (var observer in observers)
        {
            observer(_currentChanges);
        }

        _currentChanges = null;
    }

    private static ModelChangedEventArgs CreateModelChangesEventArgs(ModelChangeResult result)
    {
        return new ModelChangedEventArgs(result.OldModel, result.NewModel, result.Changes);
    }
}
