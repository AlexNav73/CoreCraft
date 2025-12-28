using System.Collections;
using System.Linq.Expressions;
using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using LinqToDB;

namespace CoreCraft.Storage.Linq2Db;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TProperties"></typeparam>
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
    /// 
    /// </summary>
    /// <param name="table"></param>
    /// <param name="info"></param>
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
    public async Task<TEntity> AddAsync(TProperties properties, CancellationToken token = default)
    {
        var inserted = await _table.InsertWithOutputAsync(properties, token);

        CacheImpl(inserted);

        return inserted.EntityId;
    }

    /// <inheritdoc />
    public async Task<TProperties?> ModifyAsync(TEntity entity, Expression<Func<TProperties, TProperties>> modifier, CancellationToken token = default)
    {
        var result = await _table
            .Where(x => x.EntityId == entity)
            .UpdateWithOutputAsync(modifier, (deleted, inserted) => inserted, token);

        var properties = result.SingleOrDefault();

        if (properties is not null)
        {
            CacheImpl(properties);
        }

        return properties;
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
    public Task<TProperties?> GetAsync(TEntity entity, CancellationToken token = default)
    {
        if (_cache.TryGetValue(entity, out var properties))
        {
            return Task.FromResult<TProperties?>(properties);
        }

        return _table.FirstOrDefaultAsync(p => p.EntityId == entity, token);
    }

    /// <inheritdoc />
    public Task<bool> ContainsAsync(TEntity entity, CancellationToken token = default)
    {
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
                    await RemoveAsync(change.Entity, token);
                    await AddAsync(change.NewData!, token);
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
