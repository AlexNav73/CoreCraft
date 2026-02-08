using System.Collections;
using System.Linq.Expressions;
using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using LinqToDB;
using LinqToDB.Async;

namespace CoreCraft.Storage.Linq2Db;

/// <summary>
///     A lazy collection implementation that delegates storage to a Linq2DB `ITable`.
/// </summary>
/// <typeparam name="TEntity">The domain entity type used as an identifier.</typeparam>
/// <typeparam name="TProperties">The properties record type stored in the database.</typeparam>
public sealed class LazyCollection<TEntity, TProperties> :
    IMutableLazyCollection<TEntity, TProperties>,
    IMutableState<ILazyCollection<TEntity, TProperties>>,
    IEntityCache<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties, IHaveEntityId<TEntity>
{
    private readonly ITable<TProperties> _table;
    private readonly Dictionary<TEntity, TProperties> _cache = [];

    /// <inheritdoc />
    public Type ElementType => _table.ElementType;

    /// <inheritdoc />
    public Expression Expression => _table.Expression;

    /// <inheritdoc />
    public IQueryProvider Provider => _table.Provider;

    /// <inheritdoc />
    public CollectionInfo Info { get; }

    /// <summary>
    ///     Initializes a new instance of `LazyCollection`.
    /// </summary>
    /// <param name="info">Collection metadata describing property schema for this collection.</param>
    /// <param name="table">Linq2DB table used as the persistent storage for this collection's properties.</param>
    public LazyCollection(CollectionInfo info, ITable<TProperties> table)
    {
        _table = table;

        Info = info;
    }

    public ILazyCollection<TEntity, TProperties> AsReadOnly()
    {
        return this;
    }

    /// <inheritdoc />
    public TEntity Add(TProperties properties)
    {
        var inserted = _table.InsertWithOutput(properties);

        CacheImpl(inserted);

        return inserted.EntityId;
    }

    /// <inheritdoc />
    public async Task<TEntity> AddAsync(TProperties properties, CancellationToken token = default)
    {
        var inserted = await _table.InsertWithOutputAsync(properties, token);

        CacheImpl(inserted);

        return inserted.EntityId;
    }

    /// <inheritdoc />
    public TProperties? Modify(TEntity entity, Expression<Func<TProperties, TProperties>> modifier)
    {
        var result = _table
            .Where(x => x.EntityId == entity)
            .UpdateWithOutput(modifier, (deleted, inserted) => inserted);

        var properties = result.SingleOrDefault();

        if (properties is not null)
        {
            CacheImpl(properties);
        }

        return properties;
    }

    /// <inheritdoc />
    public async Task<TProperties?> ModifyAsync(TEntity entity, Expression<Func<TProperties, TProperties>> modifier, CancellationToken token = default)
    {
        var result = _table
            .Where(x => x.EntityId == entity)
            .UpdateWithOutputAsync(modifier, (deleted, inserted) => inserted);

        var properties = await result.SingleOrDefaultAsync(token);

        if (properties is not null)
        {
            CacheImpl(properties);
        }

        return properties;
    }

    /// <inheritdoc />
    public void Remove(TEntity entity)
    {
        if (_cache.ContainsKey(entity))
        {
            _cache.Remove(entity);
        }

        _table.Where(p => p.EntityId == entity).Delete();
    }

    /// <inheritdoc />
    public Task RemoveAsync(TEntity entity, CancellationToken token = default)
    {
        if (_cache.ContainsKey(entity))
        {
            _cache.Remove(entity);
        }

        return _table.Where(p => p.EntityId == entity)
            .DeleteAsync(token);
    }

    /// <inheritdoc />
    public TProperties? Get(TEntity entity)
    {
        if (_cache.TryGetValue(entity, out var properties))
        {
            return properties;
        }

        return _table.FirstOrDefault(p => p.EntityId == entity);
    }

    /// <inheritdoc />
    public Task<TProperties?> GetAsync(TEntity entity, CancellationToken token = default)
    {
        if (_cache.TryGetValue(entity, out var properties))
        {
            return Task.FromResult<TProperties?>(properties);
        }

        return _table.FirstOrDefaultAsync(p => p.EntityId == entity, token);
    }

    /// <inheritdoc />
    public bool Contains(TEntity entity)
    {
        return _cache.ContainsKey(entity) || _table.Any(p => p.EntityId == entity);
    }

    /// <inheritdoc />
    public Task<bool> ContainsAsync(TEntity entity, CancellationToken token = default)
    {
        if (_cache.ContainsKey(entity))
        {
            return Task.FromResult(true);
        }

        return _table.AnyAsync(p => p.EntityId == entity, token);
    }

    /// <summary>
    /// Applies changes from a collection change set to the underlying database table.
    /// This method executes database operations for each change. For Modify actions the current row is replaced by the new data.
    /// </summary>
    public async Task ApplyAsync(ICollectionChangeSet<TEntity, TProperties> changeSet, CancellationToken token = default)
    {
        foreach (var change in changeSet)
        {
            switch (change.Action)
            {
                case CollectionAction.Add:
                    await AddAsync(change.NewData!, token);
                    break;
                case CollectionAction.Remove:
                    await RemoveAsync(change.Entity, token);
                    break;
                case CollectionAction.Modify:
                    await UpdateAsync(change.Entity, change.OldData!, change.NewData!, token);
                    break;
                default:
                    throw new NotSupportedException($"An action [{change.Action}] is not supported.");
            }
        }
    }

    void IEntityCache<TEntity, TProperties>.Cache(TProperties properties)
    {
        CacheImpl(properties);
    }

    /// <inheritdoc />
    public IEnumerator<TProperties> GetEnumerator()
    {
        throw new NotSupportedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private async Task UpdateAsync(TEntity entity, TProperties oldData, TProperties newData, CancellationToken token = default)
    {
        var oldDataBag = new PropertiesBag();
        oldData.WriteTo(oldDataBag);
        var newDataBag = new PropertiesBag();
        newData.WriteTo(newDataBag);
        var diff = oldDataBag.Compare(newDataBag);

        var query = _table.Where(p => p.EntityId == entity).AsUpdatable();

        foreach (var pair in diff)
        {
            query = query.Set(
                GetPropertyExpression(pair.Key),
                GetValueExpression(pair.Value));
        }

        var result = query.UpdateWithOutputAsync((deleted, updated) => updated);

        var updatedProperties = await result.SingleOrDefaultAsync(token);
        if (updatedProperties is not null)
        {
            CacheImpl(updatedProperties);
        }
    }

    private Expression<Func<TProperties, object?>> GetPropertyExpression(string property)
    {
        var parameter = Expression.Parameter(typeof(TProperties), "p");
        
        var expression = Expression.Lambda<Func<TProperties, object?>>(
            Expression.Convert(Expression.PropertyOrField(parameter, property), typeof(object)),
            parameter
        );

        return expression;
    }

    private Expression<Func<TProperties, object?>> GetValueExpression(object? value)
    {
        var expression = Expression.Lambda<Func<TProperties, object?>>(
            Expression.Convert(Expression.Constant(value, value?.GetType() ?? typeof(object)), typeof(object)),
            Expression.Parameter(typeof(TProperties), "p")
        );

        return expression;
    }

    private void CacheImpl(TProperties properties)
    {
        if (_cache.ContainsKey(properties.EntityId))
        {
            _cache[properties.EntityId] = properties;
        }
        else
        {
            _cache.Add(properties.EntityId, properties);
        }
    }
}
