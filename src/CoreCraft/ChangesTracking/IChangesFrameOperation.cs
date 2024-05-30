namespace CoreCraft.ChangesTracking;

/// <summary>
///     Defines visitor methods for processing collection and relation changes within a changes frame.
///     This interface follows the Visitor Pattern.
/// </summary>
public interface IChangesFrameOperation
{
    /// <summary>
    ///     Performs an operation on a collection change set within a changes frame.
    ///     This method is intended to be called by a visitor implementing this interface.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity being modified in the collection.</typeparam>
    /// <typeparam name="TProperties">The type of the properties that were changed in the collection.</typeparam>
    /// <param name="collection">An instance of <see cref="ICollectionChangeSet{TEntity, TProperties}"/> containing the collection changes.</param>
    /// <remarks>
    ///     This method follows the Visitor Pattern. It allows a visitor object to perform specific
    ///     operations on collection changes within a changes frame. The provided `collection` parameter
    ///     contains information about the specific changes that occurred, such as additions, removals,
    ///     or modifications to properties within the collection.
    /// </remarks>
    void OnCollection<TEntity, TProperties>(
        ICollectionChangeSet<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Performs an operation on a relation change set within a changes frame.
    ///     This method is intended to be called by a visitor implementing this interface.
    /// </summary>
    /// <typeparam name="TParent">The type of the parent entity in the relation.</typeparam>
    /// <typeparam name="TChild">The type of the child entity in the relation.</typeparam>
    /// <param name="relation">An instance of <see cref="IRelationChangeSet{TParent, TChild}"/> containing the relation changes.</param>
    /// <remarks>
    ///     This method follows the Visitor Pattern. It allows a visitor object to perform specific
    ///     operations on relation changes within a changes frame. The provided `relation` parameter
    ///     contains information about the specific changes that occurred.
    /// </remarks>
    void OnRelation<TParent, TChild>(
        IRelationChangeSet<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity;
}
