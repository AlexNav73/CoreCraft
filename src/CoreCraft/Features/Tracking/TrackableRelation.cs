﻿using System.Collections;
using System.Diagnostics;
using CoreCraft.ChangesTracking;
using CoreCraft.Persistence;

namespace CoreCraft.Features.Tracking;

/// <inheritdoc cref="IMutableRelation{TParent, TChild}"/>
[DebuggerDisplay("{_relation}")]
public sealed class TrackableRelation<TParent, TChild> :
    IMutableRelation<TParent, TChild>,
    IMutableState<IRelation<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{
    private readonly IRelationChangeSet<TParent, TChild> _changes;
    private readonly IMutableRelation<TParent, TChild> _relation;

    /// <summary>
    ///     Ctor
    /// </summary>
    public TrackableRelation(
        IRelationChangeSet<TParent, TChild> changesCollection,
        IMutableRelation<TParent, TChild> modelRelation)
    {
        _changes = changesCollection;
        _relation = modelRelation;
    }

    /// <inheritdoc cref="IHaveInfo{T}.Info" />
    public RelationInfo Info => _relation.Info;

    /// <inheritdoc cref="IMutableState{T}.AsReadOnly()" />
    public IRelation<TParent, TChild> AsReadOnly()
    {
        return ((IMutableState<IRelation<TParent, TChild>>)_relation).AsReadOnly();
    }

    /// <inheritdoc cref="IMutableRelation{TParent, TChild}.Add(TParent, TChild)" />
    public void Add(TParent parent, TChild child)
    {
        _relation.Add(parent, child);
        _changes.Add(RelationAction.Linked, parent, child);
    }

    /// <inheritdoc cref="IMutableRelation{TParent, TChild}.Remove(TParent)" />
    public void Remove(TParent parent)
    {
        foreach (var child in _relation.Children(parent))
        {
            _changes.Add(RelationAction.Unlinked, parent, child);
        }

        _relation.Remove(parent);
    }

    /// <inheritdoc cref="IMutableRelation{TParent, TChild}.Remove(TParent, TChild)" />
    public void Remove(TParent parent, TChild child)
    {
        _relation.Remove(parent, child);
        _changes.Add(RelationAction.Unlinked, parent, child);
    }

    /// <inheritdoc cref="IMutableRelation{TParent, TChild}.Load(IRepository, IEnumerable{TParent}, IEnumerable{TChild})" />
    public void Load(IRepository repository, IEnumerable<TParent> parents, IEnumerable<TChild> children)
    {
        if (_relation.Any())
        {
            return;
        }

        repository.Load(this, parents, children);
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.ContainsParent(TParent)" />
    public bool ContainsParent(TParent entity)
    {
        return _relation.ContainsParent(entity);
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.ContainsChild(TChild)" />
    public bool ContainsChild(TChild entity)
    {
        return _relation.ContainsChild(entity);
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.Contains(TParent, TChild)" />
    public bool Contains(TParent parent, TChild child)
    {
        return _relation.Contains(parent, child);
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.Children(TParent)" />
    public IEnumerable<TChild> Children(TParent parent)
    {
        return _relation.Children(parent);
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.Parents(TChild)" />
    public IEnumerable<TParent> Parents(TChild child)
    {
        return _relation.Parents(child);
    }

    /// <inheritdoc cref="ICopy{T}.Copy" />
    public IRelation<TParent, TChild> Copy()
    {
        throw new InvalidOperationException("Relation can't be copied because it is attached to changes tracking system");
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.Save(IRepository)" />
    public void Save(IRepository repository)
    {
        _relation.Save(repository);
    }

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator" />
    public IEnumerator<TParent> GetEnumerator()
    {
        return _relation.GetEnumerator();
    }

    /// <inheritdoc cref="IEnumerable.GetEnumerator" />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
