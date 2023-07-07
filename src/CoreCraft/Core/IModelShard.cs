namespace CoreCraft.Core;

/// <summary>
///     A base interface for a model shard.
/// </summary>
/// <remarks>
///     A model is similar to the database for the application which stores
///     it's data in-memory. Each model (like a database) contains multiple
///     shards (tables) which is a self-contained collection of data grouped
///     be the same meaning and purpose. The <see cref="IModelShard"/> interface
///     is just a marker interface that helps to store shard, but to retrieve
///     a concrete model shard - a concrete type (interface) must be used.
/// </remarks>
public interface IModelShard
{
}
