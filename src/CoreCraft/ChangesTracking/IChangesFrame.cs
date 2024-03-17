namespace CoreCraft.ChangesTracking;

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
    ///     Does the changes frame contain any changes
    /// </summary>
    /// <returns>True - if there are some changes inside the <see cref="IChangesFrame"/></returns>
    bool HasChanges();
}
