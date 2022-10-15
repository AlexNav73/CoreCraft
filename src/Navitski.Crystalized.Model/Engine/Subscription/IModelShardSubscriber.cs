using Navitski.Crystalized.Model.Engine.ChangesTracking;
#if NETSTANDARD2_0_OR_GREATER
using System.Linq.Expressions;
#elif NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Navitski.Crystalized.Model.Engine.Subscription;

/// <summary>
///     Provides a way to subscribe to model shard changes
/// </summary>
/// <typeparam name="T">A changes frame of the given model shard</typeparam>
public interface IModelShardSubscriber<T> : ISubscriber<T>
    where T : class, IChangesFrame
{
#if NET5_0_OR_GREATER
    /// <summary>
    ///     Choose a collection to receive it's changes
    /// </summary>
    /// <typeparam name="TEntity">An entity type</typeparam>
    /// <typeparam name="TProperties">A type of a properties</typeparam>
    /// <param name="accessor">A function to access a collection property</param>
    /// <param name="expression">A string representation of accessor function</param>
    /// <returns>A subscriber for the collection changes</returns>
    ICollectionSubscriber<TEntity, TProperties> With<TEntity, TProperties>(
        Func<T, ICollectionChangeSet<TEntity, TProperties>> accessor,
        [CallerArgumentExpression("accessor")] string expression = "")
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Choose a relation to receive it's changes
    /// </summary>
    /// <typeparam name="TParent">A type of parent entity</typeparam>
    /// <typeparam name="TChild">A type of child entity</typeparam>
    /// <param name="accessor">A function to access a relation property</param>
    /// <param name="expression">A string representation of accessor function</param>
    /// <returns>A subscriber for the relation changes</returns>
    IRelationSubscriber<TParent, TChild> With<TParent, TChild>(
        Func<T, IRelationChangeSet<TParent, TChild>> accessor,
        [CallerArgumentExpression("accessor")] string expression = "")
        where TParent : Entity
        where TChild : Entity;

#elif NETSTANDARD2_0_OR_GREATER
    /// <summary>
    ///     Choose a collection to receive it's changes
    /// </summary>
    /// <typeparam name="TEntity">An entity type</typeparam>
    /// <typeparam name="TProperties">A type of a properties</typeparam>
    /// <param name="accessor">An expression of a collection property</param>
    /// <returns>A subscriber for the collection changes</returns>
    ICollectionSubscriber<TEntity, TProperties> With<TEntity, TProperties>(
        Expression<Func<T, ICollectionChangeSet<TEntity, TProperties>>> accessor)
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Choose a relation to receive it's changes
    /// </summary>
    /// <typeparam name="TParent">A type of parent entity</typeparam>
    /// <typeparam name="TChild">A type of child entity</typeparam>
    /// <param name="accessor">An expression of a relation property</param>
    /// <returns>A subscriber for the relation changes</returns>
    IRelationSubscriber<TParent, TChild> With<TParent, TChild>(
        Expression<Func<T, IRelationChangeSet<TParent, TChild>>> accessor)
        where TParent : Entity
        where TChild : Entity;
#endif
}
