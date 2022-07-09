using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Commands;
using Navitski.Crystalized.Model.Engine.Exceptions;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;
using Navitski.Crystalized.Model.Engine.Subscription;

namespace Navitski.Crystalized.Model.Engine;

/// <summary>
///     A base class for model implementation
/// </summary>
public abstract class DomainModel : IDomainModel, ICommandRunner
{
    private readonly View _view;
    private readonly IScheduler _scheduler;
    private readonly ModelSubscriber _root;

    private volatile Message<IModelChanges>? _currentChanges;

    /// <summary>
    ///     Ctor
    /// </summary>
    protected DomainModel(IEnumerable<IModelShard> shards, IScheduler scheduler)
    {
        _view = new View(shards);
        _scheduler = scheduler;
        _root = new ModelSubscriber();
    }

    /// <summary>
    ///     Raised after all subscribers handled changes
    /// </summary>
    protected virtual void OnModelChanged(Message<IModelChanges> message)
    {
    }

    /// <inheritdoc cref="IModelShardAccessor.Shard{T}"/>
    public T Shard<T>() where T : IModelShard
    {
        return _view.UnsafeModel.Shard<T>();
    }

    /// <inheritdoc cref="IDomainModel.Subscribe(Action{Message{IModelChanges}})"/>
    public IDisposable Subscribe(Action<Message<IModelChanges>> onModelChanges)
    {
        var subscription = _root.Subscribe(onModelChanges);
        if (_currentChanges != null)
        {
            onModelChanges(_currentChanges);
        }

        return subscription;
    }

    /// <summary>
    ///     Provides a precise subscription mode to subscribe to a specific part of the model
    /// </summary>
    /// <param name="builder">A subscription builder</param>
    public IDisposable Subscribe(Func<IModelSubscriber, IDisposable> builder)
    {
        var subscription = builder(_root);

        if (_currentChanges != null)
        {
            var message = new Message<IModelChanges>(_currentChanges.OldModel, _currentChanges.NewModel, _currentChanges.Changes);
            var tempSubscriber = new ModelSubscriber();

            using (builder(tempSubscriber))
            {
                tempSubscriber.Push(message);
            }
        }

        return subscription;
    }

    /// <summary>
    ///     Saves a list of model changes
    /// </summary>
    /// <param name="storage">A storage to write</param>
    /// <param name="path">A path of a file</param>
    /// <param name="changes">A list of changes</param>
    /// <param name="token">Cancellation token</param>
    /// <exception cref="ModelSaveException">Throws when an error occurred while saving the model</exception>
    protected Task Save(IStorage storage, string path, IEnumerable<IModelChanges> changes, CancellationToken token = default)
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
            var eventArgs = CreateModelChangesMessage(result);

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
            var eventArgs = CreateModelChangesMessage(result);

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
            var eventArgs = CreateModelChangesMessage(result);
            
            NotifySubscribers(eventArgs);
            OnModelChanged(eventArgs);
        }
    }

    private void NotifySubscribers(Message<IModelChanges> message)
    {
        _currentChanges = message;

        _root.Push(message);

        _currentChanges = null;
    }

    private static Message<IModelChanges> CreateModelChangesMessage(ModelChangeResult result)
    {
        return new Message<IModelChanges>(result.OldModel, result.NewModel, result.Changes);
    }
}
