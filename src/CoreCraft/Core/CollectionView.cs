using System.Collections;
using CoreCraft.ChangesTracking;
using CoreCraft.Persistence;
using CoreCraft.Subscription;

namespace CoreCraft.Core;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TShard"></typeparam>
public sealed class ViewBuilder<TShard>
    where TShard : IModelShard
{
    private readonly DomainModel _model;
    private readonly ModelSubscription _modelSubscription;
    private readonly Change<IModelChanges>? _currentChanges;

    internal ViewBuilder(DomainModel model, ModelSubscription modelSubscription, Change<IModelChanges>? currentChanges)
    {
        _model = model;
        _modelSubscription = modelSubscription;
        _currentChanges = currentChanges;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TProperties"></typeparam>
    /// <param name="accessor"></param>
    /// <returns></returns>
    public ICollectionView<TEntity, TProperties> Create<TEntity, TProperties>(Func<TShard, ICollection<TEntity, TProperties>> accessor)
        where TEntity : Entity
        where TProperties : Properties
    {
        var newView = new CollectionView<TShard, TEntity, TProperties>(_model.Shard<TShard>(), accessor);
        var view = _modelSubscription.SubscribeView(newView);

        if (_currentChanges != null)
        {
            view.OnNext(_currentChanges);
        }

        return view;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    /// <typeparam name="TChild"></typeparam>
    /// <param name="accessor"></param>
    /// <returns></returns>
    public IRelationView<TParent, TChild> Create<TParent, TChild>(Func<TShard, IRelation<TParent, TChild>> accessor)
        where TParent : Entity
        where TChild : Entity
    {
        var newView = new RelationView<TShard, TParent, TChild>(_model.Shard<TShard>(), accessor);
        var view = _modelSubscription.SubscribeView(newView);

        if (_currentChanges != null)
        {
            view.OnNext(_currentChanges);
        }

        return view;
    }
}

internal abstract class DataView : DisposableBase, IObserver<Change<IModelChanges>>
{
    internal IDisposable? Subscription { get; set; }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public abstract void OnNext(Change<IModelChanges> change);

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
public interface ICollectionView<TEntity, TProperties> : ICollection<TEntity, TProperties>, IDisposable
    where TEntity : Entity
    where TProperties : Properties
{
}

internal sealed class CollectionView<TShard, TEntity, TProperties> : DataView, ICollectionView<TEntity, TProperties>
    where TShard : IModelShard
    where TEntity : Entity
    where TProperties : Properties
{
    private readonly Func<TShard, ICollection<TEntity, TProperties>> _accessor;

    private volatile ICollection<TEntity, TProperties> _collection;

    internal CollectionView(TShard shard, Func<TShard, ICollection<TEntity, TProperties>> accessor)
    {
        _accessor = accessor;
        _collection = accessor(shard);
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

    public override void OnNext(Change<IModelChanges> change)
    {
        var collection = _accessor(change.NewModel.Shard<TShard>());

        Interlocked.Exchange(ref _collection, collection);
    }

    public override int GetHashCode()
    {
        return (typeof(TShard), typeof(TEntity), typeof(TProperties)).GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is CollectionView<TShard, TEntity, TProperties>;
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TParent"></typeparam>
/// <typeparam name="TChild"></typeparam>
public interface IRelationView<TParent, TChild> : IRelation<TParent, TChild>, IDisposable
    where TParent : Entity
    where TChild : Entity
{
}

internal sealed class RelationView<TShard, TParent, TChild> : DataView, IRelationView<TParent, TChild>
    where TShard : IModelShard
    where TParent : Entity
    where TChild : Entity
{
    private readonly Func<TShard, IRelation<TParent, TChild>> _accessor;

    private volatile IRelation<TParent, TChild> _relation;

    internal RelationView(TShard shard, Func<TShard, IRelation<TParent, TChild>> accessor)
    {
        _accessor = accessor;
        _relation = accessor(shard);
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

    public override void OnNext(Change<IModelChanges> change)
    {
        var relation = _accessor(change.NewModel.Shard<TShard>());

        Interlocked.Exchange(ref _relation, relation);
    }
}
