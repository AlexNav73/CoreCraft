namespace CoreCraft.Core;

/// <summary>
///     An interface which provides query mechanism for <see cref="IMutableModelShard"/> retrieval
/// </summary>
/// <remarks>
///     This interface can be used to retrieve only mutable model shards.
/// </remarks>
public interface IMutableModel : IModel
{
    /// <summary>
    ///     Retrieves the mutable model shard by it's type
    /// </summary>
    /// <typeparam name="T">A type of a mutable model shard</typeparam>
    /// <returns>Model shard</returns>
    new T Shard<T>() where T : IMutableModelShard;
}
