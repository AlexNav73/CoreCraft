using System.Collections;
using System.Diagnostics.CodeAnalysis;
using CoreCraft.ChangesTracking;
using CoreCraft.Persistence;
using CoreCraft.Subscription;
using CoreCraft.Subscription.Builders;

namespace CoreCraft.Views;

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
