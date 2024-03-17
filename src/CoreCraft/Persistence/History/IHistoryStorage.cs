using CoreCraft.ChangesTracking;

namespace CoreCraft.Persistence.History;

/// <summary>
///     Provides methods to store and retrieve model changes history.
/// </summary>
public interface IHistoryStorage
{
    /// <summary>
    ///     Saves the provided collection of model changes to the history storage.
    /// </summary>
    /// <param name="modelChanges">
    ///     An enumerable collection of <see cref="IModelChanges"/>objects
    ///     representing changes from different model shards.
    /// </param>
    /// <remarks>
    ///     This method persists the changes that have occurred in various
    ///     model shards (<see cref="IModelChanges"/>) since the model was created or last saved.
    /// </remarks>
    void Save(IEnumerable<IModelChanges> modelChanges);

    /// <summary>
    ///     Loads changes for the specified model shards from the storage.
    /// </summary>
    /// <param name="modelShards">
    ///     An enumerable collection of <see cref="IModelShard"/> objects
    ///     representing the shards to load changes history for.
    /// </param>
    /// <returns>
    ///     An enumerable collection of <see cref="IModelChanges"/> objects
    ///     representing the loaded historical changes.
    /// </returns>
    /// <remarks>
    ///     This method retrieves previously saved changes associated with
    ///     the provided model shards from the history storage.
    /// </remarks>
    IEnumerable<IModelChanges> Load(IEnumerable<IModelShard> modelShards);
}
