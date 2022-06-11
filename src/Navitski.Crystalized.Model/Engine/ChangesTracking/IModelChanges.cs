namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <summary>
///     A collection of changes for a model
/// </summary>
/// <remarks>
///     An entry point is a model. Model has a collection of <see cref="IModelShard"/>s.
///     When model changes, a new instance of <see cref="IModelChanges"/> is created which
///     holds a collection of <see cref="IChangesFrame"/>s for each <see cref="IModelShard"/>.
///     Each <see cref="IChangesFrame"/> contain <see cref="ICollectionChangeSet{TEntity, TProperties}"/>s
///     and <see cref="IRelationChangeSet{TParent, TChild}"/>s to hold collection's and relation's
///     changes respectively. To query a specific change of an entity, <see cref="IModelChanges"/> must be used.
///     By calling <see cref="IModelChanges.TryGetFrame{T}(out T)"/> it is possible to access changes
///     of a specific <see cref="IModelShard"/> and by using properties of an <see cref="IChangesFrame"/>
///     a specific collection or relation can be queried.
/// </remarks>
public interface IModelChanges
{
    /// <summary>
    ///     Queries a specific <see cref="IChangesFrame"/> by the type
    /// </summary>
    /// <typeparam name="T">Expected type of <see cref="IChangesFrame"/></typeparam>
    /// <param name="frame">An instance of a changes frame with a specific type</param>
    /// <returns>True - if changes frame is found and matches the given type</returns>
    bool TryGetFrame<T>(out T? frame)
        where T : class, IChangesFrame;

    /// <summary>
    ///     If there were some changes
    /// </summary>
    /// <returns></returns>
    bool HasChanges();
}
