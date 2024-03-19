using CoreCraft.ChangesTracking;

namespace CoreCraft.Persistence.History;

/// <summary>
///     Provides methods to save and load model changes history.
/// </summary>
public interface IHistoryRepository
{
    /// <summary>
    ///     Saves changes made to a collection of entities with specific properties.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity being modified.</typeparam>
    /// <typeparam name="TProperties">The type of the properties that were changed.</typeparam>
    /// <param name="changeId">
    ///     A unique identifier for the change set, representing the user action that caused the modifications.
    /// </param>
    /// <param name="changes">A collection of changes to be saved.</param>
    void Save<TEntity, TProperties>(
        long changeId,
        ICollectionChangeSet<TEntity, TProperties> changes)
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Saves changes made to a relation between parent and child entities.
    /// </summary>
    /// <typeparam name="TParent">The type of the parent entity.</typeparam>
    /// <typeparam name="TChild">The type of the child entity.</typeparam>
    /// <param name="changeId">
    ///     A unique identifier for the change set, representing the user action that caused the modifications.
    /// </param>
    /// <param name="changes">A collection of changes to be saved.</param>
    void Save<TParent, TChild>(
        long changeId,
        IRelationChangeSet<TParent, TChild> changes)
        where TParent : Entity
        where TChild : Entity;

    /// <summary>
    ///     Loads changes previously associated with a specific user action.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity being modified.</typeparam>
    /// <typeparam name="TProperties">The type of the properties that were changed.</typeparam>
    /// <param name="changeId">The identifier of the change set to load, representing a specific user action.</param>
    /// <param name="changes">A collection to populate with the loaded changes.</param>
    void Load<TEntity, TProperties>(
        long changeId,
        ICollectionChangeSet<TEntity, TProperties> changes)
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Loads changes previously associated with a specific user action.
    /// </summary>
    /// <typeparam name="TParent">The type of the parent entity.</typeparam>
    /// <typeparam name="TChild">The type of the child entity.</typeparam>
    /// <param name="changeId">The identifier of the change set to load, representing a specific user action.</param>
    /// <param name="changes">A collection to populate with the loaded changes.</param>
    void Load<TParent, TChild>(
        long changeId,
        IRelationChangeSet<TParent, TChild> changes)
        where TParent : Entity
        where TChild : Entity;
}
