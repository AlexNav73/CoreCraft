using System.Collections;
using System.Diagnostics;

namespace CoreCraft.Core;

/// <inheritdoc cref="IRelation{TParent, TChild}"/>
[DebuggerDisplay(@"Parent [{_parentToChildRelations}] Children [{_childToParentRelations}]")]
public sealed class Relation<TParent, TChild> :
    IMutableRelation<TParent, TChild>,
    IMutableState<IRelation<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{
    private readonly IMapping<TParent, TChild> _parentToChildRelations;
    private readonly IMapping<TChild, TParent> _childToParentRelations;

    /// <summary>
    ///     Ctor
    /// </summary>
    public Relation(
        string id,
        IMapping<TParent, TChild> parentToChildRelation,
        IMapping<TChild, TParent> childToParentRelation)
    {
        _parentToChildRelations = parentToChildRelation;
        _childToParentRelations = childToParentRelation;

        Id = id;
    }

    /// <inheritdoc cref="IHaveId.Id" />
    public string Id { get; }

    /// <inheritdoc cref="IMutableState{T}.AsReadOnly()" />
    public IRelation<TParent, TChild> AsReadOnly()
    {
        return this;
    }

    /// <inheritdoc cref="IMutableRelation{TParent, TChild}.Add(TParent, TChild)"/>
    public void Add(TParent parent, TChild child)
    {
        _parentToChildRelations.Add(parent, child);
        _childToParentRelations.Add(child, parent);
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.ContainsParent(TParent)"/>
    public bool ContainsParent(TParent entity)
    {
        return _parentToChildRelations.Contains(entity);
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.ContainsChild(TChild)"/>
    public bool ContainsChild(TChild entity)
    {
        return _childToParentRelations.Contains(entity);
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.Children(TParent)"/>
    public IEnumerable<TChild> Children(TParent parent)
    {
        return _parentToChildRelations.Children(parent);
    }

    /// <inheritdoc cref="IRelation{TParent, TChild}.Parents(TChild)"/>
    public IEnumerable<TParent> Parents(TChild child)
    {
        return _childToParentRelations.Children(child);
    }

    /// <inheritdoc cref="IMutableRelation{TParent, TChild}.Remove(TParent, TChild)"/>
    public void Remove(TParent parent, TChild child)
    {
        _parentToChildRelations.Remove(parent, child);
        _childToParentRelations.Remove(child, parent);
    }

    /// <inheritdoc cref="ICopy{T}.Copy"/>
    public IRelation<TParent, TChild> Copy()
    {
        return new Relation<TParent, TChild>(
            Id,
            _parentToChildRelations.Copy(),
            _childToParentRelations.Copy());
    }

    /// <inheritdoc />
    public IEnumerator<TParent> GetEnumerator()
    {
        return _parentToChildRelations.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
