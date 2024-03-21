using CoreCraft.ChangesTracking;
using CoreCraft.Exceptions;
using CoreCraft.Persistence;
using CoreCraft.Scheduling;
using CoreCraft.Subscription;

namespace CoreCraft;

/// <summary>
///     A domain model which will immediately save all changes after they happened
/// </summary>
public class AutoSaveDomainModel : DomainModel
{
    private readonly IStorage _storage;

    /// <summary>
    ///     Ctor
    /// </summary>
    public AutoSaveDomainModel(
        IEnumerable<IModelShard> modelShards,
        IScheduler scheduler,
        IStorage storage)
        : base(modelShards, scheduler)
    {
        _storage = storage;
    }

    /// <summary>
    ///     Loads the model
    /// </summary>
    public async Task Load()
    {
        await Load(_storage);
    }

    /// <inheritdoc/>
    protected override async void OnModelChanged(Change<IModelChanges> change)
    {
        // TODO(#10): Saving performed in the thread pool's thread and if some
        // sequential changes come, saving order of thees changes is unpredictable.
        // Currently, changes' frequency is low and every change have enough time
        // for saving. In future, changes could be more frequent and it is necessary
        // to queue changes for saving or batch them in one big change
        await Update(_storage, change.Hunk);
    }

    private async Task Update(IStorage storage, IModelChanges changes)
    {
        try
        {
            if (changes.HasChanges())
            {
                await Scheduler.RunParallel(() => storage.Update(changes), CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            throw new ModelSaveException("Model update has failed", ex);
        }
    }
}
