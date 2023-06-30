using CoreCraft.Engine.Features;

namespace CoreCraft.Engine.Core;

/// <summary>
///     Features provide wrappers to wrap collections and relations to add new behavior.
/// </summary>
/// <remarks>
///     Every time a mutable model shard is created, it takes collections and relations
///     from the read-only model shard and wraps them using features. Each feature adds
///     new behavior to the collections and relations which is not visible to the user,
///     but it gives model shard new abilities.<br/>
///     For example:<br/>
///     <see cref="CoWFeature"/> (added out-of-the-box) copies collections/relations
///     when user tries to modify them. This feature helps to make a mutable model shard
///     independent from the original model shard (creates a snapshot which can be modified
///     even in the another thread).<br/>
///     <see cref="TrackableFeature"/> (added out-of-the-box) records all changes to the
///     collections and relation to be able to notify user about these changes.
/// </remarks>
public interface IFeature
{
    /// <summary>
    ///     Wraps a collection and returns a new collection with adjusted behavior
    /// </summary>
    /// <typeparam name="TEntity">A type of an entity</typeparam>
    /// <typeparam name="TProperties">A type of properties</typeparam>
    /// <param name="context">A context with utility methods</param>
    /// <param name="collection">A base collection which will be wrapped in this method</param>
    /// <returns>A new collection with adjusted behavior</returns>
    IMutableCollection<TEntity, TProperties> Decorate<TEntity, TProperties>(
        IFeatureContext context,
        IMutableCollection<TEntity, TProperties> collection)
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Wraps a relation and returns a new relation with adjusted behavior
    /// </summary>
    /// <typeparam name="TParent">A type of a parent entity</typeparam>
    /// <typeparam name="TChild">A type of a child entity</typeparam>
    /// <param name="context">A context with utility methods</param>
    /// <param name="relation">A base relation which will be wrapped in this method</param>
    /// <returns>A new relation with adjusted behavior</returns>
    IMutableRelation<TParent, TChild> Decorate<TParent, TChild>(
        IFeatureContext context,
        IMutableRelation<TParent, TChild> relation)
        where TParent : Entity
        where TChild : Entity;
}
