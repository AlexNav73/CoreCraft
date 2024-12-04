using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CoreCraft.ChangesTracking;
using CoreCraft.Persistence;
using CoreCraft.Subscription;
using CoreCraft.Subscription.Builders;

namespace CoreCraft.Core;

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

[ExcludeFromCodeCoverage]
internal abstract class DataView<TFrame> : DisposableBase, IObserver<Change<TFrame>>
    where TFrame : class, IChangesFrame
{
    internal IDisposable? Subscription { get; set; }

    public virtual void OnCompleted()
    {
    }

    public virtual void OnError(Exception error)
    {
    }

    public abstract void OnNext(Change<TFrame> change);

    protected override void DisposeManagedObjects()
    {
        Subscription?.Dispose();
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TProperties"></typeparam>
public interface ICollectionView<TEntity, TProperties> :
    ICollection<TEntity, TProperties>,
    ICollectionSubscriptionBuilder<TEntity, TProperties>,
    IDisposable
    where TEntity : Entity
    where TProperties : Properties
{
}

[ExcludeFromCodeCoverage]
internal sealed class CollectionView<TShard, TFrame, TEntity, TProperties> : DataView<TFrame>, ICollectionView<TEntity, TProperties>
    where TShard : IModelShard
    where TFrame : class, IChangesFrame
    where TEntity : Entity
    where TProperties : Properties
{
    private readonly Func<TShard, ICollection<TEntity, TProperties>> _accessor;
    private readonly Func<ICollectionSubscriptionBuilder<TEntity, TProperties>> _builderFactory;

    private volatile ICollection<TEntity, TProperties> _collection;

    internal CollectionView(
        ICollection<TEntity, TProperties> collection,
        Func<TShard, ICollection<TEntity, TProperties>> accessor,
        Func<ICollectionSubscriptionBuilder<TEntity, TProperties>> builderFactory)
    {
        _collection = collection;
        _accessor = accessor;
        _builderFactory = builderFactory;
    }

    public int Count => _collection.Count;

    public CollectionInfo Info => _collection.Info;

    public bool Contains(TEntity entity)
    {
        return _collection.Contains(entity);
    }

    public ICollection<TEntity, TProperties> Copy()
    {
        return _collection.Copy();
    }

    public TProperties Get(TEntity entity)
    {
        return _collection.Get(entity);
    }

    public IEnumerator<TEntity> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    public IEnumerable<(TEntity entity, TProperties properties)> Pairs()
    {
        return _collection.Pairs();
    }

    public void Save(IRepository repository)
    {
        _collection.Save(repository);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    public override void OnNext(Change<TFrame> change)
    {
        var collection = _accessor(change.NewModel.Shard<TShard>());

        Interlocked.Exchange(ref _collection, collection);
    }

    public IDisposable Bind(IObserver<Change<CollectionChangeGroups<TEntity, TProperties>>> observer)
    {
        return _builderFactory().Bind(observer);
    }

    public IDisposable Bind(TEntity entity, IObserver<IEntityChange<TEntity, TProperties>> observer)
    {
        return _builderFactory().Bind(entity, observer);
    }

    public IDisposable Subscribe(IObserver<Change<ICollectionChangeSet<TEntity, TProperties>>> observer)
    {
        return _builderFactory().Subscribe(observer);
    }

    public override int GetHashCode()
    {
        return (typeof(TShard), typeof(TFrame), typeof(TEntity), typeof(TProperties)).GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is CollectionView<TShard, TFrame, TEntity, TProperties>;
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TParent"></typeparam>
/// <typeparam name="TChild"></typeparam>
public interface IRelationView<TParent, TChild> :
    IRelation<TParent, TChild>,
    IRelationSubscriptionBuilder<TParent, TChild>,
    IDisposable
    where TParent : Entity
    where TChild : Entity
{
}

[ExcludeFromCodeCoverage]
internal sealed class RelationView<TShard, TFrame, TParent, TChild> : DataView<TFrame>, IRelationView<TParent, TChild>
    where TShard : IModelShard
    where TFrame : class, IChangesFrame
    where TParent : Entity
    where TChild : Entity
{
    private readonly Func<TShard, IRelation<TParent, TChild>> _accessor;
    private readonly Func<IRelationSubscriptionBuilder<TParent, TChild>> _builderFactory;

    private volatile IRelation<TParent, TChild> _relation;

    internal RelationView(
        IRelation<TParent, TChild> relation,
        Func<TShard, IRelation<TParent, TChild>> accessor,
        Func<IRelationSubscriptionBuilder<TParent, TChild>> builderFactory)
    {
        _relation = relation;
        _accessor = accessor;
        _builderFactory = builderFactory;
    }

    public RelationInfo Info => _relation.Info;

    public IEnumerable<TChild> Children(TParent parent)
    {
        return _relation.Children(parent);
    }

    public bool Contains(TParent parent, TChild child)
    {
        return _relation.Contains(parent, child);
    }

    public bool ContainsChild(TChild entity)
    {
        return _relation.ContainsChild(entity);
    }

    public bool ContainsParent(TParent entity)
    {
        return _relation.ContainsParent(entity);
    }

    public IRelation<TParent, TChild> Copy()
    {
        return _relation.Copy();
    }

    public IEnumerator<TParent> GetEnumerator()
    {
        return _relation.GetEnumerator();
    }

    public IEnumerable<TParent> Parents(TChild child)
    {
        return _relation.Parents(child);
    }

    public void Save(IRepository repository)
    {
        _relation.Save(repository);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _relation.GetEnumerator();
    }

    public override void OnNext(Change<TFrame> change)
    {
        var relation = _accessor(change.NewModel.Shard<TShard>());

        Interlocked.Exchange(ref _relation, relation);
    }

    public IDisposable Subscribe(IObserver<Change<IRelationChangeSet<TParent, TChild>>> observer)
    {
        return _builderFactory().Subscribe(observer);
    }

    public override int GetHashCode()
    {
        return (typeof(TShard), typeof(TFrame), typeof(TParent), typeof(TChild)).GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is RelationView<TShard, TFrame, TParent, TChild>;
    }
}
