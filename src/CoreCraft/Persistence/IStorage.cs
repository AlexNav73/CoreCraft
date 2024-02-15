using CoreCraft.ChangesTracking;
using CoreCraft.Persistence.Lazy;

namespace CoreCraft.Persistence;

/// <summary>
///     A top level storage for a whole model
/// </summary>
/// <remarks>
///     The implementation of the interface will be responsible for storing and loading
///     data of the whole model. Inside a <see cref="IStorage"/> implementation all
///     model shards will be stored or loaded.
/// </remarks>
public interface IStorage
{
    /// <summary>
    ///     Updates existing stored data by applying new changes to them
    /// </summary>
    /// <param name="modelChanges">A model shards' changes happened since model creation or last save</param>
    void Update(IEnumerable<IChangesFrame> modelChanges);

    /// <summary>
    ///     Saves the whole model
    /// </summary>
    /// <remarks>
    ///     Use this method to save model data to the empty storage (for example SQLite database)
    /// </remarks>
    /// <param name="modelShards">A collection of model shards to store</param>
    void Save(IEnumerable<IModelShard> modelShards);

    /// <summary>
    ///     Loads all data to the model
    /// </summary>
    /// <remarks>
    ///     After loading is done, all the data will be published as a changes, so
    ///     that the application can react on loaded data
    /// </remarks>
    /// <param name="modelShards">A collection of model shards which can be loaded</param>
    /// <param name="force">
    ///     (Optional) A boolean indicating whether to force loading all model shards (even if they are marked as "lazy")
    ///     together with their collections and relations (even if they are marked as "deferLoading").
    /// </param>
    void Load(IEnumerable<IMutableModelShard> modelShards, bool force = false);

    /// <summary>
    ///     Loads only the specified part of the domain model data.
    /// </summary>
    /// <remarks>
    ///     Collections can be marked as lazy using the "deferLoading" property.
    ///     If a collection is marked as "deferLoading," it means that the collection
    ///     will not be loaded during the regular <see cref="DomainModel.Load(IStorage, bool, CancellationToken)"/> invocation.
    ///     Instead, the user can decide when to load the collection. In the case of relations,
    ///     a relation is marked as "deferLoading" when at least one of the collections (parent or child)
    ///     is marked as "deferLoading" and can only be loaded after the parent and child collections have been
    ///     loaded.
    /// </remarks>
    void Load(ILazyLoader loader);
}
