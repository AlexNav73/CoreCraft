namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     A common interface to retrieve a model shard
///     from different representations of the same model.
/// </summary>
/// <remarks>
///     A model which is available all the time (implemented <see cref="IDomainModel"/>)
///     has different type comparing the model which is provided to the command or
///     in the model changes notification. To give the user the same experience of
///     retrieving a model shard from all of these types the <see cref="IModelShardAccessor"/>
///     was introduced.
/// </remarks>
public interface IModelShardAccessor
{
    /// <summary>
    ///     Retrieves the model shard by the type from the model
    /// </summary>
    /// <typeparam name="T">A type of model shard</typeparam>
    /// <returns>Model shard</returns>
    T Shard<T>() where T : IModelShard;
}
