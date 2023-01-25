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
public abstract class DomainModel : IDomainModel
{
    private readonly View _view;
    private readonly IScheduler _scheduler;
    private readonly ModelSubscriber _modelSubscriber;

    private volatile Change<IModelChanges>? _currentChanges;

    /// <summary>
    ///     Ctor
    /// </summary>
    protected DomainModel(IEnumerable<IModelShard> shards, IScheduler scheduler)
    {
        _view = new View(shards);
        _scheduler = scheduler;
        _modelSubscriber = new ModelSubscriber();
    }

    /// <summary>
    ///     Raised after all subscribers handled changes
    /// </summary>
    protected virtual void OnModelChanged(Change<IModelChanges> change)
    {
    }

    /// <inheritdoc cref="IModelShardAccessor.Shard{T}"/>
    public T Shard<T>() where T : IModelShard
    {
        return _view.UnsafeModel.Shard<T>();
    }

    /// <inheritdoc cref="IDomainModel.Subscribe(Action{Change{IModelChanges}})"/>
    public IDisposable Subscribe(Action<Change<IModelChanges>> onModelChanges)
    {
        var subscription = _modelSubscriber.By(onModelChanges);

        if (_currentChanges != null)
        {
            onModelChanges(_currentChanges);
        }

        return subscription;
    }

    /// <inheritdoc cref="IDomainModel.SubscribeTo{T}(Func{IModelShardSubscriber{T}, IDisposable})"/>
    public IDisposable SubscribeTo<T>(Func<IModelShardSubscriber<T>, IDisposable> builder)
         where T : class, IChangesFrame
    {
        var subscription = builder(_modelSubscriber.GetOrCreateSubscriberFor<T>());

        if (_currentChanges != null)
        {
            var tempSubscriber = new ModelSubscriber();

            using (builder(tempSubscriber.GetOrCreateSubscriberFor<T>()))
            {
                tempSubscriber.Publish(_currentChanges);
            }
        }

        return subscription;
    }

    /// <inheritdoc cref="IDomainModel.Run{T}(Action{T, CancellationToken}, CancellationToken)"/>
    public async Task Run<T>(Action<T, CancellationToken> command, CancellationToken token = default)
        where T : IModelShard
    {
        await Run((m, t) => command(m.Shard<T>(), t), token);
    }

    /// <inheritdoc cref="IDomainModel.Run(ICommand, CancellationToken)"/>
    public async Task Run(ICommand command, CancellationToken token = default)
    {
        await Run(command.Execute, token);
    }

    /// <inheritdoc cref="IDomainModel.Run(Action{IModel, CancellationToken}, CancellationToken)"/>
    public async Task Run(Action<IModel, CancellationToken> command, CancellationToken token = default)
    {
        var snapshot = _view.CreateTrackableSnapshot();

        try
        {
            await _scheduler.Enqueue(() => command(snapshot, token), token);
        }
        catch (Exception ex)
        {
            throw new CommandInvocationException($"Command execution failed. Command {command.GetType()}", ex);
        }

        if (snapshot.Changes.HasChanges())
        {
            var result = _view.ApplySnapshot(snapshot);
            var eventArgs = CreateChangeObject(result, snapshot.Changes);

            NotifySubscribers(eventArgs);
            OnModelChanged(eventArgs);
        }
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
        try
        {
            if (changes.Count > 0)
            {
                var merged = MergeChanges(changes);

                if (merged.HasChanges())
                {
                    return _scheduler.RunParallel(() => storage.Update(path, merged), token);
                }
            }

            return Task.CompletedTask;
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
        // Store a reference to the model before start saving task. It is safe to use UnsafeModel here
        // because we are storing IModel object in the RunParallel delegate, but not the reference to the
        // view. When the UnsafeModel stored in the local variable all changes happened after, will not
        // change stored model (instead a reference to the model in the _view will be replaced with a reference
        // to the new model, leaving old reference and model untouched). This is exact behavior we need, because
        // when Save is executed it should save state at this moment, but not when storage.Save is executed.
        var model = _view.UnsafeModel;

        try
        {
            return _scheduler.RunParallel(() => storage.Save(path, model), token);
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

        if (snapshot.Changes.HasChanges())
        {
            var result = _view.ApplySnapshot(snapshot);
            var eventArgs = CreateChangeObject(result, snapshot.Changes);

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

            var result = _view.ApplySnapshot(snapshot);
            var changeObject = CreateChangeObject(result, changes);

            NotifySubscribers(changeObject);
        }
    }

    private void NotifySubscribers(Change<IModelChanges> change)
    {
        _currentChanges = change;

        _modelSubscriber.Publish(change);

        _currentChanges = null;
    }

    private static Change<IModelChanges> CreateChangeObject(ModelChangeResult result, IWritableModelChanges changes)
    {
        return new Change<IModelChanges>(result.OldModel, result.NewModel, changes);
    }

    private static IWritableModelChanges MergeChanges(IReadOnlyList<IModelChanges> changes)
    {
        var merged = (IWritableModelChanges)changes[0];
        for (var i = 1; i < changes.Count; i++)
        {
            merged = merged.Merge(changes[i]);
        }

        return merged;
    }
}
