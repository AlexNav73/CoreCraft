using System.Runtime.CompilerServices;
using CoreCraft.ChangesTracking;
using CoreCraft.Subscription.Builders;

namespace CoreCraft.Views;

/// <summary>
///     Provides methods for creating views of collections and relations within a domain model.
/// </summary>
/// <remarks>
///     Views for collections and relations are essential for observing and responding to changes
///     in the domain model. These views are not actual collections or relations; instead, they reference
///     the originals. When the model changes, these references are updated, ensuring that views always
///     reflect the latest state. Users can retain references to any view or model shard view indefinitely,
///     and these views will always remain consistent with the data in the domain model.
/// </remarks>
/// <typeparam name="TShard">The model shard type.</typeparam>
/// <typeparam name="TFrame">The changes frame type.</typeparam>
public sealed class ViewBuilder<TShard, TFrame>
    where TShard : IModelShard
    where TFrame : class, IChangesFrame
{
    private readonly IDomainModel _model;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ViewBuilder{TShard, TFrame}"/> class.
    /// </summary>
    /// <param name="model">The domain model instance.</param>
    internal ViewBuilder(IDomainModel model)
    {
        _model = model;
    }

    /// <summary>
    ///     Creates a collection view and subscribes it to model changes.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TProperties">The properties type.</typeparam>
    /// <param name="accessor">A function to access the collection.</param>
    /// <param name="changesAccessor">A function to access the collection's change set.</param>
    /// <param name="expression">The string representation of the <paramref name="changesAccessor"/> function.</param>
    /// <returns>The created collection view.</returns>
    public ICollectionView<TEntity, TProperties> Create<TEntity, TProperties>(
        Func<TShard, ICollection<TEntity, TProperties>> accessor,
        Func<TFrame, ICollectionChangeSet<TEntity, TProperties>> changesAccessor,
        [CallerArgumentExpression(nameof(changesAccessor))] string expression = "")
        where TEntity : Entity
        where TProperties : Properties
    {
        var builder = (CollectionSubscriptionBuilder<TFrame, TEntity, TProperties>)_model
            .For<TFrame>()
            .With(changesAccessor, expression);
        var newView = new CollectionView<TShard, TFrame, TEntity, TProperties>(
            accessor(_model.Shard<TShard>()),
            accessor,
            () => _model.For<TFrame>().With(changesAccessor, expression));

        return builder.SubscribeView(newView);
    }

    /// <summary>
    ///     Creates a relation view and subscribes it to model changes.
    /// </summary>
    /// <typeparam name="TParent">The parent entity type.</typeparam>
    /// <typeparam name="TChild">The child entity type.</typeparam>
    /// <param name="accessor">A function to access the relation.</param>
    /// <param name="changesAccessor">A function to access the relation's change set.</param>
    /// <param name="expression">The string representation of the <paramref name="changesAccessor"/> function.</param>
    /// <returns>The created relation view.</returns>
    public IRelationView<TParent, TChild> Create<TParent, TChild>(
        Func<TShard, IRelation<TParent, TChild>> accessor,
        Func<TFrame, IRelationChangeSet<TParent, TChild>> changesAccessor,
        [CallerArgumentExpression(nameof(changesAccessor))] string expression = "")
        where TParent : Entity
        where TChild : Entity
    {
        var builder = (RelationSubscriptionBuilder<TFrame, TParent, TChild>)_model
            .For<TFrame>()
            .With(changesAccessor, expression);
        var newView = new RelationView<TShard, TFrame, TParent, TChild>(
            accessor(_model.Shard<TShard>()),
            accessor,
            () => _model.For<TFrame>().With(changesAccessor, expression));

        return builder.SubscribeView(newView);
    }
}
