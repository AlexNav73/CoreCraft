namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <summary>
///     A common interface for all change frames for each model shard.
/// </summary>
/// <remarks>
///     Each model shard has it's own associated changes frame. When model
///     shard modified, a new changes frame created to store modifications
///     for further analysis. Changes frames are generated for each model shard
///     and have the same collections and relation as in original model shard, but
///     instead of <see cref="ICollection{TEntity, TProperties}"/> and <see cref="IRelation{TParent, TChild}"/>
///     change set types are used.
/// </remarks>
public interface IChangesFrame
{
    /// <summary>
    ///     Retrieves a collection's changes set
    /// </summary>
    /// <typeparam name="TEntity">A type of a collection's entity</typeparam>
    /// <typeparam name="TProperty">A type of a collection's properties</typeparam>
    /// <param name="collection">A collection to query a change set</param>
    /// <returns>A collection's change set</returns>
    ICollectionChangeSet<TEntity, TProperty> Get<TEntity, TProperty>(ICollection<TEntity, TProperty> collection)
        where TEntity : Entity
        where TProperty : Properties;

    /// <summary>
    ///     Retrieves a relation's changes set
    /// </summary>
    /// <typeparam name="TParent">A type of a relation's parent entity</typeparam>
    /// <typeparam name="TChild">A type of a relation's child entity</typeparam>
    /// <param name="relation">A relation to query a change set</param>
    /// <returns>A relation's change set</returns>
    IRelationChangeSet<TParent, TChild> Get<TParent, TChild>(IRelation<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity;

    /// <summary>
    ///     Does the changes frame contain any changes
    /// </summary>
    /// <returns>True - if there are some changes inside the <see cref="IChangesFrame"/></returns>
    bool HasChanges();
}
