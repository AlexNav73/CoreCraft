namespace CoreCraft.Engine.Core;

/// <summary>
///     An mutable counterpart of a <see cref="ICollection{TEntity, TProperties}"/> interface
/// </summary>
/// <remarks>
///     When a <see cref="Commands.ICommand"/> executes it receives a mutable model.
///     When a <see cref="Commands.ICommand"/> finishes, model notifies all it's subscribers
///     that model has been changed and passes a new and old model to the subscriber. When subscriber
///     receives models it can only read them, because all modifications must happen inside the commands
///     to keep track of changes. Only commands can provide an access to a mutable model shard.
///     A mutable model shard has mutable collections and relations so they can be modified
///     and all modifications will be recorded and available when command finishes.
/// </remarks>
/// <typeparam name="TEntity">A type of an entity</typeparam>
/// <typeparam name="TProperties">A type of a properties</typeparam>
public interface IMutableCollection<TEntity, TProperties> : ICollection<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties
{
    /// <summary>
    ///     Adds a new properties to the collection with a generated entity
    /// </summary>
    /// <param name="properties">A new properties</param>
    /// <returns>New entity</returns>
    TEntity Add(TProperties properties);

    /// <summary>
    ///     Adds a new properties with a given entity id and initialization function
    /// </summary>
    /// <param name="id">An entity id</param>
    /// <param name="init">An initialization function for a properties</param>
    /// <returns>New entity</returns>
    TEntity Add(Guid id, Func<TProperties, TProperties> init);

    /// <summary>
    ///     Adds a new properties by the given entity
    /// </summary>
    /// <param name="entity">An entity</param>
    /// <param name="properties">A properties</param>
    /// <exception cref="Exceptions.DuplicateKeyException">Throws when trying to add an entity which is already present in the collection</exception>
    void Add(TEntity entity, TProperties properties);

    /// <summary>
    ///     Modifies properties of the given entity
    /// </summary>
    /// <param name="entity">An entity</param>
    /// <param name="modifier">A function which takes old properties and returns new properties with modifications</param>
    /// <exception cref="KeyNotFoundException">Throws when trying to modify an entity which is not present in the collection</exception>
    void Modify(TEntity entity, Func<TProperties, TProperties> modifier);

    /// <summary>
    ///     Removes entity with properties from the collection
    /// </summary>
    /// <param name="entity">An entity</param>
    /// <exception cref="KeyNotFoundException">Throws when trying to remove an entity which is not present in the collection</exception>
    void Remove(TEntity entity);
}
