using CoreCraft.ChangesTracking;
using System.Runtime.CompilerServices;

namespace CoreCraft.Subscription.Builders;

/// <summary>
///     Subscription builder which provides a way to subscribe to model shard changes
/// </summary>
/// <typeparam name="T">A changes frame of the given model shard</typeparam>
public interface IModelShardSubscriptionBuilder<T> : IObservable<Change<T>>
    where T : class, IChangesFrame
{
    /// <summary>
    ///     Choose a collection to receive it's changes
    /// </summary>
    /// <typeparam name="TEntity">An entity type</typeparam>
    /// <typeparam name="TProperties">A type of a properties</typeparam>
    /// <param name="accessor">A function to access a collection property</param>
    /// <param name="expression">A string representation of accessor function</param>
    /// <returns>A subscription builder for the collection changes</returns>
    ICollectionSubscriptionBuilder<TEntity, TProperties> With<TEntity, TProperties>(
        Func<T, ICollectionChangeSet<TEntity, TProperties>> accessor,
        [CallerArgumentExpression(nameof(accessor))] string expression = "")
        where TEntity : Entity
        where TProperties : Properties;

    /// <summary>
    ///     Choose a relation to receive it's changes
    /// </summary>
    /// <typeparam name="TParent">A type of parent entity</typeparam>
    /// <typeparam name="TChild">A type of child entity</typeparam>
    /// <param name="accessor">A function to access a relation property</param>
    /// <param name="expression">A string representation of accessor function</param>
    /// <returns>A subscription builder for the relation changes</returns>
    IRelationSubscriptionBuilder<TParent, TChild> With<TParent, TChild>(
        Func<T, IRelationChangeSet<TParent, TChild>> accessor,
        [CallerArgumentExpression(nameof(accessor))] string expression = "")
        where TParent : Entity
        where TChild : Entity;
}
