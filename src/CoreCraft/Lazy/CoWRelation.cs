using System.Collections;
using System.Diagnostics;

namespace CoreCraft.Lazy;

/// <summary>
///     Relation wrapper which will copy inner relation only before write action
/// </summary>
/// <remarks>
///     Read actions will not cause copying of the inner relation
/// </remarks>
[DebuggerDisplay("{_copy ?? _relation}")]
public sealed class CoWRelation<TParent, TChild> :
    IMutableRelation<TParent, TChild>,
    IMutableState<IRelation<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{
    private readonly IRelation<TParent, TChild> _relation;

    private IMutableRelation<TParent, TChild>? _copy;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="relation">An inner relation</param>
    public CoWRelation(IRelation<TParent, TChild> relation)
    {
        _relation = relation;
    }

    /// <inheritdoc cref="IHaveInfo{T}.Info" />
    public RelationInfo Info => _relation.Info;

    /// <inheritdoc cref="IMutableState{T}.AsReadOnly()" />
    public IRelation<TParent, TChild> AsReadOnly()
    {
        return ((IMutableState<IRelation<TParent, TChild>>)(_copy ?? _relation)).AsReadOnly();
    }

    /// <inheritdoc cref="IMutableRelation{TParent, TChild}.Add(TParent, TChild)" />
    public void Add(TParent parent, TChild child)
    {
        _copy ??= (IMutableRelation<TParent, TChild>)_relation.Copy();

        _copy.Add(parent, child);
    }

    /// <inheritdoc cref="IMutableRelation{TParent, TChild}.Remove(TParent, TChild)" />
    public void Remove(TParent parent, TChild child)
    {
        _copy ??= (IMutableRelation<TParent, TChild>)_relation.Copy();

        _copy.Remove(parent, child);
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.ContainsParent(TParent)" />
    public bool ContainsParent(TParent entity)
    {
        return (_copy ?? _relation).ContainsParent(entity);
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.ContainsChild(TChild)" />
    public bool ContainsChild(TChild entity)
    {
        return (_copy ?? _relation).ContainsChild(entity);
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.AreLinked(TParent, TChild)" />
    public bool AreLinked(TParent parent, TChild child)
    {
        return (_copy ?? _relation).AreLinked(parent, child);
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.Children(TParent)" />
    public IEnumerable<TChild> Children(TParent parent)
    {
        return (_copy ?? _relation).Children(parent);
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.Parents(TChild)" />
    public IEnumerable<TParent> Parents(TChild child)
    {
        return (_copy ?? _relation).Parents(child);
    }

    /// <inheritdoc cref="ICopy{T}.Copy" />
    public IRelation<TParent, TChild> Copy()
    {
        throw new InvalidOperationException("Cannot copy a CoW Relation.");
    }

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator" />
    public IEnumerator<TParent> GetEnumerator()
    {
        return (_copy ?? _relation).GetEnumerator();
    }

    /// <inheritdoc cref="IEnumerable.GetEnumerator" />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return (_copy ?? _relation).GetEnumerator();
    }
}
