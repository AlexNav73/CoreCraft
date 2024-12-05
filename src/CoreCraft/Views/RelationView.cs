using System.Collections;
using System.Diagnostics.CodeAnalysis;
using CoreCraft.ChangesTracking;
using CoreCraft.Persistence;
using CoreCraft.Subscription;
using CoreCraft.Subscription.Builders;

namespace CoreCraft.Views;

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
