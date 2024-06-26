﻿using CoreCraft.Exceptions;
using System.Collections;
using System.Diagnostics;

namespace CoreCraft.Core;

/// <summary>
///     Describes One-To-One relationships between entities. For more information see <see cref="IMapping{TParent, TChild}"/>
/// </summary>
/// <typeparam name="TParent">A parent entity type</typeparam>
/// <typeparam name="TChild">A child entity type</typeparam>
[DebuggerDisplay("Count = {_relation.Keys.Count}")]
public sealed class OneToOne<TParent, TChild> : IMapping<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    private readonly IDictionary<TParent, TChild> _relation;

    /// <summary>
    ///     Ctor
    /// </summary>
    public OneToOne() : this(new Dictionary<TParent, TChild>())
    {
    }

    private OneToOne(IDictionary<TParent, TChild> relation)
    {
        _relation = relation;
    }

    /// <inheritdoc cref="IMapping{TParent, TChild}.Add(TParent, TChild)"/>
    public void Add(TParent parent, TChild child)
    {
        if (_relation.ContainsKey(parent))
        {
            throw new DuplicatedRelationException($"Linking {parent} with {child} has failed");
        }

        _relation.Add(parent, child);
    }

    /// <inheritdoc cref="IMapping{TParent, TChild}.Contains(TParent)"/>
    public bool Contains(TParent parent)
    {
        return _relation.ContainsKey(parent);
    }

    /// <inheritdoc cref="IMapping{TParent, TChild}.Contains(TParent, TChild)"/>
    public bool Contains(TParent parent, TChild child)
    {
        return _relation.TryGetValue(parent, out var c) && c == child;
    }

    /// <inheritdoc cref="IMapping{TParent, TChild}.Children(TParent)"/>
    public IEnumerable<TChild> Children(TParent parent)
    {
        if (_relation.TryGetValue(parent, out var child))
        {
            yield return child;
        }
    }

    /// <inheritdoc cref="IMapping{TParent, TChild}.Remove(TParent)"/>
    public void Remove(TParent parent)
    {
        if (_relation.ContainsKey(parent))
        {
            _relation.Remove(parent);
        }
        else
        {
            throw new MissingRelationException($"Can't remove all {parent} links");
        }
    }

    /// <inheritdoc cref="IMapping{TParent, TChild}.Remove(TParent, TChild)"/>
    public void Remove(TParent parent, TChild child)
    {
        if (_relation.TryGetValue(parent, out var c) && c.Equals(child))
        {
            _relation.Remove(parent);
        }
        else
        {
            throw new MissingRelationException($"Can't remove {parent} - {child} link");
        }
    }

    /// <inheritdoc cref="ICopy{T}.Copy"/>
    public IMapping<TParent, TChild> Copy()
    {
        return new OneToOne<TParent, TChild>(new Dictionary<TParent, TChild>(_relation));
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
