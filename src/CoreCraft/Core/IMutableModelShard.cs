using CoreCraft.Persistence;

namespace CoreCraft.Core;

/// <summary>
///     A base interface for a mutable model shard, which represents a portion of a domain model
///     similar to a database in a database management system.
/// </summary>
/// <remarks>
///     A domain model is often structured like a database management system, where the
///     application stores multiple in-memory databases (shards). Each model shard contains
///     collections (like tables) and relations (relationships between entities from different
///     collections). The <see cref="IMutableModelShard"/> interface serves as a marker for storing and
///     retrieving concrete model shards.
/// </remarks>
public interface IMutableModelShard : IModelShard
{
    /// <summary>
    ///     Gets a boolean indicating whether manual loading from the repository is required.
    /// </summary>
    /// <remarks>
    ///     This property determines whether the model shard attempts to load data
    ///     when <see cref="DomainModel.Load(IStorage, bool, CancellationToken)"/> is called.
    ///     Setting it to `true` means that all data, including non-lazy collections and relations,
    ///     must be manually loaded using the <see cref="DomainModel.Load{T}(IStorage, bool, CancellationToken)"/> method.
    ///     This can improve performance by avoiding unnecessary loading.
    /// </remarks>
    bool ManualLoadRequired { get; }

    /// <summary>
    ///     Loads data into the implementing model shard from the provided repository.
    /// </summary>
    /// <param name="repository">The repository used to load the model shard data.</param>
    /// <param name="force">
    ///     (Optional) A boolean indicating whether to force loading collections and/or relations
    ///     even if they are marked as "loadManually".
    /// </param>
    /// <remarks>
    ///     This method retrieves data for the model shard from the specified repository.
    ///     By default, this method loads non-lazy collections and relations within the shard.
    ///     To load all collections and relations, regardless of their lazy settings, set `force` to `true`.
    /// </remarks>
    void Load(IRepository repository, bool force = false);
}
