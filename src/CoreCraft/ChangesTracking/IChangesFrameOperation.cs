namespace CoreCraft.ChangesTracking;

/// <summary>
///     TODO: write documentation
/// </summary>
public interface IChangesFrameOperation
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <typeparam name="TProperties">A type of properties</typeparam>
    /// <param name="collection">A base collection which will be wrapped in this method</param>
    /// <returns>A new collection with adjusted behavior</returns>
    void OnCollection<TEntity, TProperties>(
        ICollectionChangeSet<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    /// </summary>
    /// <typeparam name="TParent">A type of a parent entity</typeparam>
    /// <typeparam name="TChild">A type of a child entity</typeparam>
    /// <param name="relation">A base relation which will be wrapped in this method</param>
    /// <returns>A new relation with adjusted behavior</returns>
    void OnRelation<TParent, TChild>(
        IRelationChangeSet<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity;
}
