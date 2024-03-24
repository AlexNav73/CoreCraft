using CoreCraft.Persistence;

namespace CoreCraft.Core;

/// <summary>
///     A base interface for a model shard.
/// </summary>
/// <remarks>
///     A domain model is similar to the database management system (like MS SQL Service)
///     for the application which stores multiple databases (shards) in-memory.
///     Each model shard (like a database) contains multiple
///     collections and relations (tables). The <see cref="IModelShard"/> interface
///     is just a marker interface that helps to store and retrieve
///     a concrete model shard.
/// </remarks>
public interface IModelShard
{
    /// <summary>
    ///     Saves the implementing model shard using the provided repository.
    /// </summary>
    /// <param name="repository">The repository used to save the model shard.</param>
    void Save(IRepository repository);
}
