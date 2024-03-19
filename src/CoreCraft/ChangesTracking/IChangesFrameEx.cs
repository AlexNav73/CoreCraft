namespace CoreCraft.ChangesTracking;

/// <summary>
///     An extension of the <see cref="IChangesFrame"/> which used internally in the <see cref="DomainModel"/>
/// </summary>
public interface IChangesFrameEx : IChangesFrame
{
    /// <summary>
    ///     Applies changes to the given model
    /// </summary>
    /// <param name="model">A target model</param>
    void Apply(IModel model);

    /// <summary>
    ///     Merges two <see cref="IChangesFrameEx"/>s into one,
    ///     reducing a number of operations (changes) stored in the <see cref="IChangesFrameEx"/>.
    /// </summary>
    /// <remarks>
    ///     It helps to optimize count of actions needed to be performed to update stored data to the latest version
    /// </remarks>
    /// <param name="frame">Changes, that have happened after the current ones</param>
    /// <returns>Merged frames by combining current frame with the newest</returns>
    IChangesFrame Merge(IChangesFrame frame);

    /// <summary>
    ///     Retrieves a collection's changes set
    /// </summary>
    /// <typeparam name="TEntity">A type of a collection's entity</typeparam>
    /// <typeparam name="TProperty">A type of a collection's properties</typeparam>
    /// <param name="collection">A collection to query a change set</param>
    /// <returns>A collection's change set</returns>
    ICollectionChangeSet<TEntity, TProperty>? Get<TEntity, TProperty>(ICollection<TEntity, TProperty> collection)
        where TEntity : Entity
        where TProperty : Properties;

    /// <summary>
    ///     Retrieves a relation's changes set
    /// </summary>
    /// <typeparam name="TParent">A type of a relation's parent entity</typeparam>
    /// <typeparam name="TChild">A type of a relation's child entity</typeparam>
    /// <param name="relation">A relation to query a change set</param>
    /// <returns>A relation's change set</returns>
    IRelationChangeSet<TParent, TChild>? Get<TParent, TChild>(IRelation<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity;

    /// <summary>
    ///     Creates a new <see cref="IChangesFrame"/> which holds the changes opposite to the original changes
    /// </summary>
    /// <returns>A new inverted changes</returns>
    IChangesFrame Invert();

    /// <summary>
    ///     TODO: write documentation
    /// </summary>
    /// <param name="operation"></param>
    void Do<T>(T operation) where T : IChangesFrameOperation;
}
