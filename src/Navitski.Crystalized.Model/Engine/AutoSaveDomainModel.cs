using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Engine.Scheduling;
using Navitski.Crystalized.Model.Engine.Subscription;

namespace Navitski.Crystalized.Model.Engine;

/// <summary>
///     A domain model which will immediately save all changes after they happened
/// </summary>
public class AutoSaveDomainModel : DomainModel
{
    private readonly IStorage _storage;
    private readonly string _path;

    /// <summary>
    ///     Ctor
    /// </summary>
    public AutoSaveDomainModel(
        IEnumerable<IModelShard> modelShards,
        IScheduler scheduler,
        IStorage storage,
        string path)
        : base(modelShards, scheduler)
    {
        _storage = storage;
        _path = path;
    }

    /// <summary>
    ///     Loads the model
    /// </summary>
    public async Task Load()
    {
        await Load(_storage, _path);
    }

    /// <inheritdoc/>
    protected override async void OnModelChanged(Message<IModelChanges> message)
    {
        // TODO(#10): Saving performed in the thread pool's thread and if some
        // sequential changes come, saving order of thees changes is unpredictable.
        // Currently, changes' frequency is low and every change have enough time
        // for saving. In future, changes could be more frequent and it is necessary
        // to queue changes for saving or batch them in one big change
        await Save(_storage, _path, new[] { message.Changes });
    }
}
