using CoreCraft.Engine.ChangesTracking;

namespace CoreCraft.Engine.Subscription.Builders;

/// <summary>
///     A collection of extensions methods for <see cref="ISubscriptionBuilder{T}"/>
/// </summary>
public static class SubscriptionBuilderExtensions
{
    /// <summary>
    ///     Subscribes to a specific model shard changes using a delegate
    /// </summary>
    /// <typeparam name="TFrame">A collection of model shard changes</typeparam>
    /// <param name="self">A model shard subscription builder</param>
    /// <param name="handler">A delegate which will handle model shard changes</param>
    /// <returns>A subscription</returns>
    public static IDisposable Subscribe<TFrame>(
        this IModelShardSubscriptionBuilder<TFrame> self,
        Action<Change<TFrame>> handler)
            where TFrame : class, IChangesFrame
    {
        return self.Subscribe(new AnonymousObserver<Change<TFrame>>(handler));
    }

    /// <summary>
    ///     Subscribes to a collection change events using a delegate
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity object</typeparam>
    /// <typeparam name="TProperties">The type of the properties associated with the entity</typeparam>
    /// <param name="self">A collection subscription builder</param>
    /// <param name="handler">A delegate which will handle collection changes</param>
    /// <returns>A subscription</returns>
    public static IDisposable Subscribe<TEntity, TProperties>(
        this ICollectionSubscriptionBuilder<TEntity, TProperties> self,
        Action<Change<ICollectionChangeSet<TEntity, TProperties>>> handler)
          where TEntity : Entity
          where TProperties : Properties
    {
        return self.Subscribe(new AnonymousObserver<Change<ICollectionChangeSet<TEntity, TProperties>>>(handler));
    }

    /// <summary>
    ///     Subscribes to a relation change events using a delegate
    /// </summary>
    /// <typeparam name="TParent">A type of parent entity</typeparam>
    /// <typeparam name="TChild">A type of child entity</typeparam>
    /// <param name="self">A relation subscription builder</param>
    /// <param name="handler">A delegate which will handle relation changes</param>
    /// <returns>A subscription</returns>
    public static IDisposable Subscribe<TParent, TChild>(
        this IRelationSubscriptionBuilder<TParent, TChild> self,
        Action<Change<IRelationChangeSet<TParent, TChild>>> handler)
          where TParent : Entity
          where TChild : Entity
    {
        return self.Subscribe(new AnonymousObserver<Change<IRelationChangeSet<TParent, TChild>>>(handler));
    }

    /// <summary>
    ///     Binds to a collection change events using a delegate
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity object</typeparam>
    /// <typeparam name="TProperties">The type of the properties associated with the entity</typeparam>
    /// <param name="self">A collection subscription builder</param>
    /// <param name="handler">A delegate which will handle collection changes</param>
    /// <returns>A subscription</returns>
    public static IDisposable Bind<TEntity, TProperties>(
        this ICollectionSubscriptionBuilder<TEntity, TProperties> self,
        Action<BindingChanges<TEntity, TProperties>> handler)
          where TEntity : Entity
          where TProperties : Properties
    {
        return self.Bind(new AnonymousObserver<BindingChanges<TEntity, TProperties>>(handler));
    }

    /// <summary>
    ///     Binds to an entity change events using a delegate
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity object</typeparam>
    /// <typeparam name="TProperties">The type of the properties associated with the entity</typeparam>
    /// <param name="self">A collection subscription builder</param>
    /// <param name="entity">An entity to subscribe to</param>
    /// <param name="handler">A delegate which will handle entity's changes</param>
    /// <returns>A subscription</returns>
    public static IDisposable Bind<TEntity, TProperties>(
        this ICollectionSubscriptionBuilder<TEntity, TProperties> self,
        TEntity entity,
        Action<IEntityChange<TEntity, TProperties>> handler)
          where TEntity : Entity
          where TProperties : Properties
    {
        return self.Bind(entity, new AnonymousObserver<IEntityChange<TEntity, TProperties>>(handler));
    }
}
