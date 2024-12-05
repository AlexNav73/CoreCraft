using System.Runtime.CompilerServices;
using CoreCraft.ChangesTracking;
using CoreCraft.Subscription.Builders;

namespace CoreCraft.Views;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TShard"></typeparam>
/// <typeparam name="TFrame"></typeparam>
public sealed class ViewBuilder<TShard, TFrame>
    where TShard : IModelShard
    where TFrame : class, IChangesFrame
{
    private readonly IDomainModel _model;

    internal ViewBuilder(IDomainModel model)
    {
        _model = model;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TProperties"></typeparam>
    /// <param name="accessor"></param>
    /// <param name="changesAccessor"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    /// <typeparam name="TChild"></typeparam>
    /// <param name="accessor"></param>
    /// <param name="changesAccessor"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
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
