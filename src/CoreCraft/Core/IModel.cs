namespace CoreCraft.Core;

/// <summary>
///     An interface which provides query mechanism for <see cref="IModelShard"/> retrieval
/// </summary>
/// <remarks>
///     This interface can be used to retrieve only read-only model shards.
/// </remarks>
public interface IModel
{
    /// <summary>
    ///     Retrieves the model shard by it's type
    /// </summary>
    /// <typeparam name="T">A type of model shard</typeparam>
    /// <returns>Model shard</returns>
    T Shard<T>() where T : IModelShard;
}
