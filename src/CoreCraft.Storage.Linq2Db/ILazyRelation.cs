using System.Collections;
using System.Linq.Expressions;
using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using LinqToDB;
using Newtonsoft.Json.Linq;

namespace CoreCraft.Storage.Linq2Db;

public sealed record Pair<TParent, TChild>(TParent Parent, TChild Child)
    where TParent : Entity
    where TChild : Entity;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TParent"></typeparam>
/// <typeparam name="TChild"></typeparam>
public interface ILazyRelation<TParent, TChild> : IQueryable<Pair<TParent, TChild>>, IHaveInfo<RelationInfo>
    where TParent : Entity
    where TChild : Entity
{
}

public interface IMutableLazyRelation<TParent, TChild> : ILazyRelation<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    Task AddAsync(TParent parent, TChild child, CancellationToken token = default);
    
    Task RemoveAsync(TParent parent, CancellationToken token = default);

    Task RemoveAsync(TParent parent, TChild child, CancellationToken token = default);

    Task ApplyAsync(IRelationChangeSet<TParent, TChild> changeSet, CancellationToken token = default);
}

public sealed class LazyRelation<TParent, TChild, TParentRelation, TChildRelation> :
    IMutableLazyRelation<TParent, TChild>,
    IMutableState<ILazyRelation<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
    where TParentRelation : IRelationType<TParent>
    where TChildRelation : IRelationType<TChild>
{
    private readonly ITable<ParentToChild<TParentRelation, TChildRelation>> _table;
    private readonly Func<TParent, TParentRelation> _parentRelationFactory;
    private readonly Func<TChild, TChildRelation> _childRelationFactory;
    private readonly IQueryable<Pair<TParent, TChild>> _query;

    /// <summary>
    ///     Ctor
    /// </summary>
    public LazyRelation(
        RelationInfo info,
        ITable<ParentToChild<TParentRelation, TChildRelation>> table,
        Func<TParent, TParentRelation> parentRelationFactory,
        Func<TChild, TChildRelation> childRelationFactory)
    {
        _table = table;
        _parentRelationFactory = parentRelationFactory;
        _childRelationFactory = childRelationFactory;
        _query = _table.Select(x => new Pair<TParent, TChild>(x.Parent.Id, x.Child.Id));

        Info = info;
    }

    /// <inheritdoc cref="IHaveInfo{T}.Info"/>
    public RelationInfo Info { get; }

    public Type ElementType => _query.ElementType;

    public Expression Expression => _query.Expression;

    public IQueryProvider Provider => _query.Provider;

    public ILazyRelation<TParent, TChild> AsReadOnly()
    {
        return this;
    }

    public Task AddAsync(TParent parent, TChild child, CancellationToken token = default)
    {
        return _table
            .AsValueInsertable()
            .Value(p => p.Parent, _parentRelationFactory(parent))
            .Value(p => p.Child, _childRelationFactory(child))
            .InsertAsync(token);
    }

    /// <inheritdoc cref="IMutableLazyRelation{TParent, TChild}.RemoveAsync(TParent, CancellationToken)"/>
    public Task RemoveAsync(TParent parent, CancellationToken token = default)
    {
        return _table
            .Where(p => p.Parent.Id == parent)
            .DeleteAsync(token);
    }

    /// <inheritdoc cref="IMutableLazyRelation{TParent, TChild}.RemoveAsync(TParent, TChild, CancellationToken)"/>
    public Task RemoveAsync(TParent parent, TChild child, CancellationToken token = default)
    {
        return _table
            .Where(p => p.Parent.Id == parent && p.Child.Id == child)
            .DeleteAsync(token);
    }

    /// <summary>
    /// Applies changes from a relation change set to this lazy relation.
    /// This will perform mapping operations (Add/Remove) on the underlying mapping abstraction.
    /// </summary>
    public async Task ApplyAsync(IRelationChangeSet<TParent, TChild> changeSet, CancellationToken token = default)
    {
        foreach (var change in changeSet)
        {
            switch (change.Action)
            {
                case RelationAction.Linked:
                    await AddAsync(change.Parent, change.Child, token);
                    break;
                case RelationAction.Unlinked:
                    await RemoveAsync(change.Parent, change.Child, token);
                    break;
                default:
                    throw new NotSupportedException($"An action [{change.Action}] is not supported.");
            }
        }
    }

    public IEnumerator<Pair<TParent, TChild>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
