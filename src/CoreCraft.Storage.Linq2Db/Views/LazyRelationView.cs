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
internal sealed class LazyRelationView<TShard, TFrame, TParent, TChild> : DataView<TFrame>, ILazyRelationView<TParent, TChild>
    where TShard : IModelShard
    where TFrame : class, IChangesFrame
    where TParent : Entity
    where TChild : Entity
{
    private readonly Func<TShard, ILazyRelation<TParent, TChild>> _accessor;
    private readonly Func<IRelationSubscriptionBuilder<TParent, TChild>> _builderFactory;

    private volatile ILazyRelation<TParent, TChild> _relation;

    internal LazyRelationView(
        ILazyRelation<TParent, TChild> relation,
        Func<TShard, ILazyRelation<TParent, TChild>> accessor,
        Func<IRelationSubscriptionBuilder<TParent, TChild>> builderFactory)
    {
        _relation = relation;
        _accessor = accessor;
        _builderFactory = builderFactory;
    }

    public RelationInfo Info => _relation.Info;

    public Type ElementType => _relation.ElementType;

    public Expression Expression => _relation.Expression;

    public IQueryProvider Provider => _relation.Provider;

    public IEnumerator<Pair<TParent, TChild>> GetEnumerator()
    {
        return _relation.GetEnumerator();
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
        return obj is LazyRelationView<TShard, TFrame, TParent, TChild>;
    }
}
