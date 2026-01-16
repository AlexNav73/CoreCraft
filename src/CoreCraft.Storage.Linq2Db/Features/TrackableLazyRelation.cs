using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Storage.Linq2Db.Extensions;
using LinqToDB.Async;

namespace CoreCraft.Storage.Linq2Db.Features;

/// <inheritdoc cref="IMutableRelation{TParent, TChild}"/>
[DebuggerDisplay("{_relation}")]
public sealed class TrackableLazyRelation<TParent, TChild> :
    IMutableLazyRelation<TParent, TChild>,
    IMutableState<ILazyRelation<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{
    private readonly IRelationChangeSet<TParent, TChild> _changes;
    private readonly IMutableLazyRelation<TParent, TChild> _relation;

    /// <summary>
    ///     Ctor
    /// </summary>
    public TrackableLazyRelation(
        IRelationChangeSet<TParent, TChild> changesCollection,
        IMutableLazyRelation<TParent, TChild> modelRelation)
    {
        _changes = changesCollection;
        _relation = modelRelation;
    }

    /// <inheritdoc cref="IHaveInfo{T}.Info" />
    public RelationInfo Info => _relation.Info;

    public Type ElementType => _relation.ElementType;
    
    public Expression Expression => _relation.Expression;
    
    public IQueryProvider Provider => _relation.Provider;

    /// <inheritdoc cref="IMutableState{T}.AsReadOnly()" />
    public ILazyRelation<TParent, TChild> AsReadOnly()
    {
        return ((IMutableState<ILazyRelation<TParent, TChild>>)_relation).AsReadOnly();
    }

    public async Task AddAsync(TParent parent, TChild child, CancellationToken token = default)
    {
        await _relation.AddAsync(parent, child, token);
        _changes.Add(RelationAction.Linked, parent, child);
    }

    public async Task RemoveAsync(TParent parent, CancellationToken token = default)
    {
        var children = await _relation.Children(parent).ToListAsync(token);
        await _relation.RemoveAsync(parent, token);
        foreach (var child in children)
        {
            _changes.Add(RelationAction.Unlinked, parent, child);
        }
    }

    public async Task RemoveAsync(TParent parent, TChild child, CancellationToken token = default)
    {
        await _relation.RemoveAsync(parent, child, token);
        _changes.Add(RelationAction.Unlinked, parent, child);
    }

    /// <inheritdoc cref="IMutableLazyRelation{TParent, TChild}.ApplyAsync(IRelationChangeSet{TParent, TChild}, CancellationToken)" />
    public Task ApplyAsync(IRelationChangeSet<TParent, TChild> changeSet, CancellationToken token = default)
    {
        throw new InvalidOperationException("Unable to apply changes to the relation");
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
