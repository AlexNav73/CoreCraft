using System.Collections;
using System.Diagnostics;

namespace Navitski.Crystalized.Model.Engine.Lazy;

/// <summary>
///     Collection wrapper which will copy inner collection only before write action
/// </summary>
/// <remarks>
///     Read actions will not cause copying of the inner collection
/// </remarks>
[DebuggerDisplay("{_copy ?? _collection}")]
public sealed class CoWCollection<TEntity, TProperties> :
    IMutableCollection<TEntity, TProperties>,
    ICanBeReadOnly<ICollection<TEntity, TProperties>>
    where TEntity : Entity
    where TProperties : Properties
{
    private readonly ICollection<TEntity, TProperties> _collection;

    private IMutableCollection<TEntity, TProperties>? _copy;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="collection">Internal collection</param>
    public CoWCollection(ICollection<TEntity, TProperties> collection)
    {
        _collection = collection;
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Count"/>
    public int Count => (_copy ?? _collection).Count;

    /// <inheritdoc cref="ICanBeReadOnly{T}.AsReadOnly()" />
    public ICollection<TEntity, TProperties> AsReadOnly()
    {
        return ((ICanBeReadOnly<ICollection<TEntity, TProperties>>)(_copy ?? _collection)).AsReadOnly();
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Add(TProperties)"/>
    public TEntity Add(TProperties properties)
    {
        _copy ??= (IMutableCollection<TEntity, TProperties>)_collection.Copy();

        return _copy.Add(properties);
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Add(Guid, Func{TProperties, TProperties})"/>
    public TEntity Add(Guid id, Func<TProperties, TProperties> init)
    {
        _copy ??= (IMutableCollection<TEntity, TProperties>)_collection.Copy();

        return _copy.Add(id, init);
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Add(TEntity, TProperties)"/>
    public void Add(TEntity entity, TProperties properties)
    {
        _copy ??= (IMutableCollection<TEntity, TProperties>)_collection.Copy();

        _copy.Add(entity, properties);
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Contains(TEntity)"/>
    public bool Contains(TEntity entity)
    {
        return (_copy ?? _collection).Contains(entity);
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Get(TEntity)"/>
    public TProperties Get(TEntity entity)
    {
        return (_copy ?? _collection).Get(entity);
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Modify(TEntity, Func{TProperties, TProperties})"/>
    public void Modify(TEntity entity, Func<TProperties, TProperties> modifier)
    {
        _copy ??= (IMutableCollection<TEntity, TProperties>)_collection.Copy();

        _copy.Modify(entity, modifier);
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Remove(TEntity)"/>
    public void Remove(TEntity entity)
    {
        _copy ??= (IMutableCollection<TEntity, TProperties>)_collection.Copy();

        _copy.Remove(entity);
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Pairs()"/>
    public IEnumerable<(TEntity entity, TProperties properties)> Pairs()
    {
        return (_copy ?? _collection).Pairs();
    }

    /// <inheritdoc cref="ICopy{T}.Copy()"/>
    public ICollection<TEntity, TProperties> Copy()
    {
        throw new InvalidOperationException("Cannot copy a CoW Collection.");
    }

    /// <inheritdoc />
    public IEnumerator<TEntity> GetEnumerator()
    {
        return (_copy ?? _collection).GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return (_copy ?? _collection).GetEnumerator();
    }
}
