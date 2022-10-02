using Navitski.Crystalized.Model.Engine.Exceptions;
using System.Collections;
using System.Diagnostics;

namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     Describes One-To-Many relationships between entities. For more information see <see cref="IMapping{TParent, TChild}"/>
/// </summary>
/// <typeparam name="TParent">A parent entity type</typeparam>
/// <typeparam name="TChild">A child entity type</typeparam>
[DebuggerDisplay("Count = {_relation.Keys.Count}")]
public sealed class OneToMany<TParent, TChild> : IMapping<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    private readonly IDictionary<TParent, HashSet<TChild>> _relation;

    /// <summary>
    ///     Ctor
    /// </summary>
    public OneToMany() : this(new Dictionary<TParent, HashSet<TChild>>())
    {
    }

    private OneToMany(IDictionary<TParent, HashSet<TChild>> relation)
    {
        _relation = relation;
    }

    /// <inheritdoc cref="IMapping{TParent, TChild}.Add(TParent, TChild)"/>
    public void Add(TParent parent, TChild child)
    {
        if (_relation.TryGetValue(parent, out var children))
        {
            if (!children.Contains(child))
            {
                children.Add(child);
            }
            else
            {
                throw new DuplicatedRelationException($"Link between {parent} and {child} already exists");
            }
        }
        else
        {
            _relation.Add(parent, new HashSet<TChild>() { child });
        }
    }

    /// <inheritdoc cref="IMapping{TParent, TChild}.Remove(TParent, TChild)"/>
    public void Remove(TParent parent, TChild child)
    {
        if (!_relation.TryGetValue(parent, out var children))
        {
            throw new MissingRelationException($"Can't remove {parent} - {child} link");
        }

        if (children.Contains(child))
        {
            children.Remove(child);
        }

        if (children.Count == 0)
        {
            _relation.Remove(parent);
        }
    }

    /// <inheritdoc cref="IMapping{TParent, TChild}.Children(TParent)"/>
    public IEnumerable<TChild> Children(TParent parent)
    {
        if (_relation.TryGetValue(parent, out var children))
        {
            return children;
        }

        return Array.Empty<TChild>();
    }

    /// <inheritdoc cref="IMapping{TParent, TChild}.Clear"/>
    public void Clear()
    {
        _relation.Clear();
    }

    /// <inheritdoc cref="ICopy{T}.Copy"/>
    public IMapping<TParent, TChild> Copy()
    {
        var relation = new Dictionary<TParent, HashSet<TChild>>();
        foreach (var pair in _relation)
        {
            relation[pair.Key] = new HashSet<TChild>(pair.Value);
        }
        return new OneToMany<TParent, TChild>(relation);
    }

    /// <inheritdoc />
    public IEnumerator<TParent> GetEnumerator()
    {
        return _relation.Keys.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
