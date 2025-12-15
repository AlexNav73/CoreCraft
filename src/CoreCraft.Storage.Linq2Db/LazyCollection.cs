using System.Collections;
using System.Linq.Expressions;
using CoreCraft.Core;
using CoreCraft.ChangesTracking;
using LinqToDB;

namespace CoreCraft.Storage.Linq2Db;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TProperties"></typeparam>
public sealed class LazyCollection<TEntity, TProperties> :
    IMutableLazyCollection<TEntity, TProperties>,
    IMutableState<ILazyCollection<TEntity, TProperties>>
    where TEntity : Entity
    where TProperties : Properties, IHaveEntityId<TEntity>
{
    private readonly ITable<TProperties> _table;

    /// <inheritdoc />
    public Type ElementType => _table.ElementType;

    /// <inheritdoc />
    public Expression Expression => _table.Expression;

    /// <inheritdoc />
    public IQueryProvider Provider => _table.Provider;

    /// <inheritdoc />
    public CollectionInfo Info { get; }

    /// <inheritdoc />
    public IQueryable<TEntity> Entities => _table.Select(p => p.EntityId);

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

        return inserted.EntityId;
    }

    /// <inheritdoc />
    public Task ModifyAsync<T>(TEntity entity, Expression<Func<TProperties, T>> property, T value, CancellationToken token = default)
    {
        return _table.Where(p => p.EntityId == entity)
            .Set(property, value)
            .UpdateAsync(token);
    }

    /// <inheritdoc />
    public Task RemoveAsync(TEntity entity, CancellationToken token = default)
    {
        return _table.Where(p => p.EntityId == entity)
            .DeleteAsync(token);
    }

    /// <inheritdoc />
    public Task<TProperties?> GetAsync(TEntity entity, CancellationToken token = default)
    {
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
                    // Insert the new data row. Use InsertWithOutputAsync to ensure inserted values are returned if needed.
                    await _table.InsertWithOutputAsync(change.NewData!, token).ConfigureAwait(false);
                    break;
                case CollectionAction.Remove:
                    await _table.Where(p => p.EntityId == change.Entity)
                        .DeleteAsync(token).ConfigureAwait(false);
                    break;
                case CollectionAction.Modify:
                    // Replace approach: delete existing row then insert new one.
                    await _table.Where(p => p.EntityId == change.Entity)
                        .DeleteAsync(token).ConfigureAwait(false);
                    await _table.InsertWithOutputAsync(change.NewData!, token).ConfigureAwait(false);
                    break;
                default:
                    throw new NotSupportedException($"An action [{change.Action}] is not supported.");
            }
        }
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
}
