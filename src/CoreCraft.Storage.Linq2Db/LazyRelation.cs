using System.Collections;
using System.Linq.Expressions;
using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using LinqToDB;
using Newtonsoft.Json.Linq;

namespace CoreCraft.Storage.Linq2Db;

public sealed class LazyRelation<TParent, TChild> :
    IMutableLazyRelation<TParent, TChild>,
    IMutableState<ILazyRelation<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{
    private readonly ITable<Pair<TParent, TChild>> _table;

    /// <summary>
    ///     Ctor
    /// </summary>
    public LazyRelation(RelationInfo info, ITable<Pair<TParent, TChild>> table)
    {
        _table = table;

        Info = info;
    }

    /// <inheritdoc cref="IHaveInfo{T}.Info"/>
    public RelationInfo Info { get; }

    public Type ElementType => _table.ElementType;

    public Expression Expression => _table.Expression;

    public IQueryProvider Provider => _table.Provider;

    public ILazyRelation<TParent, TChild> AsReadOnly()
    {
        return this;
    }

    /// <inheritdoc cref="IMutableLazyRelation{TParent, TChild}.Add(TParent, TChild)"/>
    public void Add(TParent parent, TChild child)
    {
        _table
            .AsValueInsertable()
            .Value(p => p.Parent, parent)
            .Value(p => p.Child, child)
            .Insert();
    }

    /// <inheritdoc cref="IMutableLazyRelation{TParent, TChild}.AddAsync(TParent, TChild, CancellationToken)"/>
    public Task AddAsync(TParent parent, TChild child, CancellationToken token = default)
    {
        return _table
            .AsValueInsertable()
            .Value(p => p.Parent, parent)
            .Value(p => p.Child, child)
            .InsertAsync(token);
    }

    /// <inheritdoc cref="IMutableLazyRelation{TParent, TChild}.Remove(TParent)"/>
    public void Remove(TParent parent)
    {
        _table
            .Where(p => p.Parent.Equals(parent))
            .Delete();
    }

    /// <inheritdoc cref="IMutableLazyRelation{TParent, TChild}.RemoveAsync(TParent, CancellationToken)"/>
    public Task RemoveAsync(TParent parent, CancellationToken token = default)
    {
        return _table
            .Where(p => p.Parent.Equals(parent))
            .DeleteAsync(token);
    }

    /// <inheritdoc cref="IMutableLazyRelation{TParent, TChild}.Remove(TParent, TChild)"/>
    public void Remove(TParent parent, TChild child)
    {
        _table
            .Where(p => p.Parent.Equals(parent) && p.Child.Equals(child))
            .Delete();
    }

    /// <inheritdoc cref="IMutableLazyRelation{TParent, TChild}.RemoveAsync(TParent, TChild, CancellationToken)"/>
    public Task RemoveAsync(TParent parent, TChild child, CancellationToken token = default)
    {
        return _table
            .Where(p => p.Parent.Equals(parent) && p.Child.Equals(child))
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
