using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Subscription;
using CoreCraft.Subscription.Builders;
using CoreCraft.Views;

namespace CoreCraft.Storage.Linq2Db.Views;

[ExcludeFromCodeCoverage]
internal sealed class LazyCollectionView<TShard, TFrame, TEntity, TProperties> : DataView<TFrame>, ILazyCollectionView<TEntity, TProperties>
    where TShard : IModelShard
    where TFrame : class, IChangesFrame
    where TEntity : Entity
    where TProperties : Properties, IHaveEntityId<TEntity>
{
    private readonly Func<TShard, ILazyCollection<TEntity, TProperties>> _accessor;
    private readonly Func<ICollectionSubscriptionBuilder<TEntity, TProperties>> _builderFactory;

    private volatile ILazyCollection<TEntity, TProperties> _collection;

    internal LazyCollectionView(
        ILazyCollection<TEntity, TProperties> collection,
        Func<TShard, ILazyCollection<TEntity, TProperties>> accessor,
        Func<ICollectionSubscriptionBuilder<TEntity, TProperties>> builderFactory)
    {
        _collection = collection;
        _accessor = accessor;
        _builderFactory = builderFactory;
    }

    public CollectionInfo Info => _collection.Info;

    public Type ElementType => _collection.ElementType;

    public Expression Expression => _collection.Expression;

    public IQueryProvider Provider => _collection.Provider;

    public TProperties? Get(TEntity entity)
        => _collection.Get(entity);

    public Task<TProperties?> GetAsync(TEntity entity, CancellationToken token = default)
        => _collection.GetAsync(entity, token);

    public bool Contains(TEntity entity)
        => _collection.Contains(entity);

    public Task<bool> ContainsAsync(TEntity entity, CancellationToken token = default)
        => _collection.ContainsAsync(entity, token);

    public IEnumerator<TProperties> GetEnumerator()
    {
        throw new NotImplementedException();
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
        return obj is LazyCollectionView<TShard, TFrame, TEntity, TProperties>;
    }
}
