using Navitski.Crystalized.Model.Engine.Subscription;

namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     An interface which provides query mechanism for <see cref="IModelShard"/> retrieval
/// </summary>
/// <remarks>
///     This interface can be used to retrieve both read-only and mutable model shards. It
///     depends in which context it is used. For example: if the user calls <see cref="Shard{T}"/>
///     method on <see cref="IDomainModel"/> or <see cref="Change{T}.NewModel"/> instance
///     - then a read-only model shard can be requested. If the user calls the same method inside of
///     a command - only mutable model shards can be requested.
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
