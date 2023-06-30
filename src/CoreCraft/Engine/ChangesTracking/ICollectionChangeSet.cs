namespace CoreCraft.Engine.ChangesTracking;

/// <summary>
///     A container of changes performed on the collection
/// </summary>
/// <typeparam name="TEntity">A type of an entity</typeparam>
/// <typeparam name="TProperties">A type of a properties</typeparam>
/// <remarks>
///     When a command executes, all changes to the model are recorded in change sets.
///     All change sets are combined in a single <see cref="IChangesFrame"/> which can
///     be queried for a specific change by an entity. Changes can be inverted to produce
///     "undo" changes which can revert collection to the state, before modification.
///     When needed, change sets can be applied to the collection or relation to update it
///     to the newer version
/// </remarks>
public interface ICollectionChangeSet<TEntity, TProperties> : IHaveId, IEnumerable<ICollectionChange<TEntity, TProperties>>
    where TEntity : Entity
    where TProperties : Properties
{
    /// <summary>
    ///     Adds new change record to the change set
    /// </summary>
    /// <param name="action">An action performed</param>
    /// <param name="entity">On which entity action was performed</param>
    /// <param name="oldData">The old properties of an entity</param>
    /// <param name="newData">The new properties of an entity</param>
    void Add(CollectionAction action, TEntity entity, TProperties? oldData, TProperties? newData);

    /// <summary>
    ///     Creates a new <see cref="ICollectionChangeSet{TEntity, TProperties}"/> which holds the changes opposite to the original changes
    /// </summary>
    /// <returns>A new inverted changes</returns>
    /// <exception cref="NotSupportedException">Throws when at least one change has an Action with a wrong value</exception>
    ICollectionChangeSet<TEntity, TProperties> Invert();

    /// <summary>
    ///     Applies changes to the given collection
    /// </summary>
    /// <param name="collection">A target collection</param>
    /// <exception cref="NotSupportedException">Throws when at least one change has an Action with a wrong value</exception>
    void Apply(IMutableCollection<TEntity, TProperties> collection);

    /// <summary>
    ///     If a change set is not empty
    /// </summary>
    /// <returns>True - if a change set holds some changes</returns>
    bool HasChanges();

    /// <summary>
    ///     Merges two <see cref="ICollectionChangeSet{TEntity, TProperties}"/>s into one,
    ///     reducing a number of operations (changes) stored in the <see cref="ICollectionChangeSet{TEntity, TProperties}"/>.
    /// </summary>
    /// <remarks>
    ///     It helps to optimize count of actions needed to be performed to update stored data to the latest version
    /// </remarks>
    /// <param name="changeSet">Changes, that have happened after the current ones</param>
    /// <returns>Merged changes by combining current changes with the newest</returns>
    ICollectionChangeSet<TEntity, TProperties> Merge(ICollectionChangeSet<TEntity, TProperties> changeSet);
}
