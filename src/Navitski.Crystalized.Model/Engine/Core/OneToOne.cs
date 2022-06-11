using Navitski.Crystalized.Model.Engine.Exceptions;
using System.Collections;
using System.Diagnostics;

namespace Navitski.Crystalized.Model.Engine.Core;

[DebuggerDisplay("Count = {_relation.Keys.Count}")]
public class OneToOne<TParent, TChild> : IMapping<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    private readonly IDictionary<TParent, TChild> _relation;

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

    /// <inheritdoc cref="IMapping{TParent, TChild}.Children(TParent)"/>
    public IEnumerable<TChild> Children(TParent parent)
    {
        if (_relation.TryGetValue(parent, out var child))
        {
            yield return child;
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

    /// <inheritdoc cref="IMapping{TParent, TChild}.Clear"/>
    public void Clear()
    {
        _relation.Clear();
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
