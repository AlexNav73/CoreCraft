using CoreCraft.ChangesTracking;
using CoreCraft.Commands;
using CoreCraft.Exceptions;
using CoreCraft.Features.CoW;
using CoreCraft.Features.Tracking;
using CoreCraft.Persistence;
using CoreCraft.Persistence.Lazy;
using CoreCraft.Scheduling;
using CoreCraft.Subscription;
using CoreCraft.Subscription.Builders;

namespace CoreCraft;

/// <summary>
///     A base class for model implementation
/// </summary>
public class DomainModel : IDomainModel
{
    private readonly View _view;
    private readonly IScheduler _scheduler;
    private readonly ModelSubscription _modelSubscription;

    private volatile Change<IModelChanges>? _currentChanges;

    /// <summary>
    ///     Ctor
    /// </summary>
    public DomainModel(IEnumerable<IModelShard> shards)
        : this(shards, new AsyncScheduler())
    {
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    public DomainModel(IEnumerable<IModelShard> shards, IScheduler scheduler)
    {
        _view = new View(shards);
        _scheduler = scheduler;
        _modelSubscription = new ModelSubscription();
    }

    /// <inheritdoc cref="IModel.Shard{T}"/>
    public T Shard<T>() where T : IModelShard
    {
        return _view.UnsafeModel.Shard<T>();
    }

    /// <inheritdoc cref="IDomainModel.Subscribe(Action{Change{IModelChanges}})"/>
    public IDisposable Subscribe(Action<Change<IModelChanges>> onModelChanges)
    {
        var subscription = _modelSubscription.Subscribe(new AnonymousObserver<Change<IModelChanges>>(onModelChanges));

        if (_currentChanges != null)
        {
            onModelChanges(_currentChanges);
        }

        return subscription;
    }

    /// <inheritdoc cref="IDomainModel.For{T}()"/>
    public IModelShardSubscriptionBuilder<T> For<T>()
         where T : class, IChangesFrame
    {
        return new ModelShardSubscriptionBuilder<T>(_modelSubscription.GetOrCreateSubscriptionFor<T>(), _currentChanges);
    }

    /// <inheritdoc cref="IDomainModel.Run{T}(Action{T, CancellationToken}, CancellationToken)"/>
    public async Task Run<T>(Action<T, CancellationToken> command, CancellationToken token = default)
        where T : IMutableModelShard
    {
        await Run((m, t) => command(m.Shard<T>(), t), token);
    }

    /// <inheritdoc cref="IDomainModel.Run(ICommand, CancellationToken)"/>
    public async Task Run(ICommand command, CancellationToken token = default)
    {
        await Run(command.Execute, token);
    }

    /// <inheritdoc cref="IDomainModel.Run(Action{IMutableModel, CancellationToken}, CancellationToken)"/>
    public async Task Run(Action<IMutableModel, CancellationToken> command, CancellationToken token = default)
    {
        var changes = new ModelChanges(DateTime.UtcNow.Ticks);
        var snapshot = new Snapshot(_view.UnsafeModel, [new CoWFeature(), new TrackableFeature(changes)]);

        try
        {
            await _scheduler.Enqueue(() => command(snapshot, token), token);
        }
        catch (Exception ex)
        {
            throw new CommandInvocationException($"Command execution failed. Command {command.GetType()}", ex);
        }

        if (changes.HasChanges())
        {
            var result = _view.ApplySnapshot(snapshot);
            var eventArgs = CreateChangeObject(result, changes);

            NotifySubscriptions(eventArgs);
            OnModelChanged(eventArgs);
        }
    }

    /// <summary>
    ///     Saves a model as a whole when storage is empty
    /// </summary>
    /// <param name="storage">A storage</param>
    /// <param name="token">Cancellation token</param>
    /// <exception cref="ModelSaveException">Throws when an error occurred while saving the model</exception>
    public Task Save(IStorage storage, CancellationToken token = default)
    {
        // Store a references to model shards before start saving task. It is safe to use UnsafeModel here
        // because we are storing model shards objects in the RunParallel delegate, but not the reference to the
        // view. When model shards from UnsafeModel stored in the local variable all changes happened after, will not
        // change stored model shards (instead a reference to the model in the _view will be replaced with a reference
        // to the new model, leaving old references and model shards untouched). This is exact behavior we need, because
        // when Save is executed it should save state at that moment, but not when 'storage.Save' is executed.
        var model = _view.UnsafeModel.Shards.ToArray(); // Do not remove ToArray from here!

        try
        {
            return _scheduler.RunParallel(() => storage.Save(model), token);
        }
        catch (Exception ex)
        {
            throw new ModelSaveException("Model save has failed", ex);
        }
    }

    /// <summary>
    ///     Loads the domain model data.
    /// </summary>
    /// <param name="storage">A storage.</param>
    /// <param name="force">
    ///     (Optional) A boolean indicating whether to force loading all model shards (even if they are marked as "loadManually")
    ///     together with their collections and relations (even if they are marked as "loadManually").
    /// </param>
    /// <param name="token">Cancellation token.</param>
    /// <exception cref="ModelLoadingException">Throws when an error occurred while loading the model.</exception>
    public Task Load(IStorage storage, bool force = false, CancellationToken token = default)
    {
        var changes = new ModelChanges(DateTime.UtcNow.Ticks);
        var snapshot = new LoadSnapshot(_view.UnsafeModel, new[] { new TrackableFeature(changes) });

        return Load(snapshot, changes, () => storage.Load(snapshot, force), token);
    }

    /// <summary>
    ///     Loads only the specified model shard.
    /// </summary>
    /// <remarks>
    ///     Model shards can be marked as lazy using the "loadManually" property.
    ///     If a model shard is marked as "loadManually", it means that the model shard
    ///     will not be loaded during the regular <see cref="Load(IStorage, bool, CancellationToken)"/> invocation.
    ///     Instead, the user can decide when to load the model shard.
    /// </remarks>
    /// <param name="storage">The storage from which to load the data.</param>
    /// <param name="force">
    ///     (Optional) A boolean indicating whether to force loading collections and/or relations
    ///     even if they are marked as "loadManually".
    /// </param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="ModelLoadingException">Thrown when an error occurs while loading the model.</exception>
    public Task Load<T>(IStorage storage, bool force = false, CancellationToken token = default)
        where T : IMutableModelShard
    {
        var changes = new ModelChanges(DateTime.UtcNow.Ticks);
        var snapshot = new Snapshot(_view.UnsafeModel, new[] { new TrackableFeature(changes) });
        var loader = new ModelLoader<T>(((IMutableModel)snapshot).Shard<T>(), force);

        return Load(snapshot, changes, () => storage.Load(loader), token);
    }

    /// <summary>
    ///     Loads only the specified part of the domain model data.
    /// </summary>
    /// <remarks>
    ///     Collections can be marked as lazy using the "loadManually" property.
    ///     If a collection is marked as "loadManually", it means that the collection
    ///     will not be loaded during the regular <see cref="Load(IStorage, bool, CancellationToken)"/> invocation.
    ///     Instead, the user can decide when to load the collection. In the case of relations,
    ///     a relation is marked as "loadManually" when at least one of the collections (parent or child)
    ///     is marked as "loadManually" and can only be loaded after the parent and child collections had been
    ///     loaded.
    /// </remarks>
    /// <param name="storage">The storage from which to load the data.</param>
    /// <param name="configure">An action in which the specific collections and relations can be loaded.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="ModelLoadingException">Thrown when an error occurs while loading the model.</exception>
    public Task Load<T>(IStorage storage, Func<IModelShardLoader<T>, ILazyLoader> configure, CancellationToken token = default)
        where T : IMutableModelShard
    {
        var changes = new ModelChanges(DateTime.UtcNow.Ticks);
        var snapshot = new Snapshot(_view.UnsafeModel, new[] { new TrackableFeature(changes) });
        var loader = new ModelShardLoader<T>(((IMutableModel)snapshot).Shard<T>());
        var configuration = configure(loader);

        return Load(snapshot, changes, () => storage.Load(configuration), token);
    }

    /// <summary>
    ///     Provides direct access to the underlying collection of model shards in a read-only manner.
    /// </summary>
    /// <remarks>
    ///     <b>Warning:</b>
    ///     <list type="bullet">
    ///         <item>This method exposes the model shards collection as a snapshot of its state at the time of access.</item>
    ///         <item>Subsequent changes to the model through commands or other operations might not be reflected in the returned collection.</item>
    ///         <item>It's recommended to use the <see cref="Shard{T}()"/> method for safer, up-to-date access to specific model shards.</item>
    ///         <item>Modifying the returned collection directly can lead to unexpected behavior and data inconsistencies.</item>
    ///     </list>
    /// </remarks>
    internal IReadOnlyCollection<IModelShard> UnsafeGetModelShards() => _view.UnsafeModel.Shards.ToList();

    /// <summary>
    ///     Applies changes to the model
    /// </summary>
    /// <param name="changes">Changes to apply</param>
    /// <param name="token">Cancellation token</param>
    /// <exception cref="ApplyModelChangesException">Throws when an error occurred while applying changes to the model</exception>
    internal async Task Apply(IModelChanges changes, CancellationToken token = default)
    {
        if (changes.HasChanges())
        {
            var snapshot = new Snapshot(_view.UnsafeModel, new[] { new CoWFeature() });

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

            NotifySubscriptions(changeObject);
        }
    }

    /// <summary>
    ///     Raised after all subscribers handled changes
    /// </summary>
    protected virtual void OnModelChanged(Change<IModelChanges> change)
    {
    }

    private async Task Load(ISnapshot snapshot, ModelChanges changes, Action action, CancellationToken token)
    {
        try
        {
            await _scheduler.Enqueue(action, token);
        }
        catch (Exception ex)
        {
            throw new ModelLoadingException("Model loading has failed", ex);
        }

        if (changes.HasChanges())
        {
            var result = _view.ApplySnapshot(snapshot);
            var changeObject = CreateChangeObject(result, changes);

            NotifySubscriptions(changeObject);
        }
    }

    private void NotifySubscriptions(Change<IModelChanges> change)
    {
        _currentChanges = change;

        _modelSubscription.Publish(change);

        _currentChanges = null;
    }

    private static Change<IModelChanges> CreateChangeObject(ModelChangeResult result, IModelChanges changes)
    {
        return new Change<IModelChanges>(result.OldModel, result.NewModel, changes);
    }
}
