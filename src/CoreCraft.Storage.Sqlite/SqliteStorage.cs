using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Persistence;
using CoreCraft.Persistence.History;
using CoreCraft.Persistence.Lazy;
using CoreCraft.Persistence.Operations;
using CoreCraft.Storage.Sqlite.Migrations;
using System.Data;

namespace CoreCraft.Storage.Sqlite;

/// <summary>
///     A SQLite storage implementation for the domain model
/// </summary>
public sealed class SqliteStorage : IStorage, IHistoryStorage
{
    private readonly string _path;
    private readonly MigrationRunner _migrationRunner;
    private readonly ISqliteRepositoryFactory _sqliteRepositoryFactory;
    private readonly Action<string>? _loggingAction;

    /// <summary>
    ///     Ctor
    /// </summary>
    public SqliteStorage(
        string path,
        IEnumerable<IMigration> migrations,
        Action<string>? loggingAction = null)
        : this(path, migrations, new SqliteRepositoryFactory(), loggingAction)
    {
    }

    internal SqliteStorage(
        string path,
        IEnumerable<IMigration> migrations,
        ISqliteRepositoryFactory sqliteRepositoryFactory,
        Action<string>? loggingAction = null)
    {
        _path = path;
        _migrationRunner = new MigrationRunner(migrations);
        _sqliteRepositoryFactory = sqliteRepositoryFactory;
        _loggingAction = loggingAction;
    }

    /// <inheritdoc cref="IStorage.Update(IEnumerable{IChangesFrame})"/>
    public void Update(IEnumerable<IChangesFrame> modelChanges)
    {
        Transaction(_path, repository =>
        {
            foreach (var change in modelChanges.Cast<IChangesFrameEx>())
            {
                change.Do(new UpdateChangesFrameOperation(repository));
            }
        });
    }

    /// <inheritdoc cref="IHistoryStorage.Save(IEnumerable{IModelChanges})"/>
    public void Save(IEnumerable<IModelChanges> modelChanges)
    {
        Transaction(_path, repository =>
        {
            _migrationRunner.UpdateDatabaseVersion(repository);

            repository.ExecuteNonQuery(QueryBuilder.DropTable(QueryBuilder.History.CollectionHistory));
            repository.ExecuteNonQuery(QueryBuilder.DropTable(QueryBuilder.History.RelationHistory));

            repository.ExecuteNonQuery(QueryBuilder.History.CreateCollectionTable);
            repository.ExecuteNonQuery(QueryBuilder.History.CreateRelationTable);

            foreach (var change in modelChanges)
            {
                change.Save(repository);
            }
        });
    }

    /// <inheritdoc cref="IStorage.Save(IEnumerable{IModelShard})"/>
    public void Save(IEnumerable<IModelShard> modelShards)
    {
        Transaction(_path, repository =>
        {
            _migrationRunner.UpdateDatabaseVersion(repository);

            foreach (var shard in modelShards)
            {
                shard.Save(repository);
            }
        });
    }

    /// <inheritdoc cref="IStorage.Load(IEnumerable{IMutableModelShard}, bool)"/>
    public void Load(IEnumerable<IMutableModelShard> modelShards, bool force = false)
    {
        using var repository = _sqliteRepositoryFactory.Create(_path, _loggingAction);

        _migrationRunner.Run(repository);

        foreach (var shard in modelShards.Where(x => force || !x.ManualLoadRequired))
        {
            shard.Load(repository, force);
        }
    }

    /// <inheritdoc />
    public void Load(ILazyLoader loader)
    {
        using var repository = _sqliteRepositoryFactory.Create(_path, _loggingAction);

        _migrationRunner.Run(repository);

        loader.Load(repository);
    }

    /// <inheritdoc />
    public IEnumerable<IModelChanges> Load(IEnumerable<IModelShard> modelShards)
    {
        using var repository = _sqliteRepositoryFactory.Create(_path, _loggingAction);

        _migrationRunner.Run(repository);

        repository.ExecuteNonQuery(QueryBuilder.History.CreateCollectionTable);
        repository.ExecuteNonQuery(QueryBuilder.History.CreateRelationTable);

        return repository.RestoreHistory(modelShards);
    }

    private void Transaction(string path, Action<ISqliteRepository> action)
    {
        ISqliteRepository? repository = null;
        IDbTransaction? transaction = null;

        try
        {
            repository = _sqliteRepositoryFactory.Create(path, _loggingAction);
            transaction = repository.BeginTransaction();

            action(repository);

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction?.Rollback();
            _loggingAction?.Invoke($"Exception was thrown with message: {ex.Message}");
            _loggingAction?.Invoke($"Stack trace: {ex.StackTrace}");
            throw;
        }
        finally
        {
            transaction?.Dispose();
            repository?.Dispose();
        }
    }
}
