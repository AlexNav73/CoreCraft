using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using CoreCraft.Persistence;
using CoreCraft.Storage.Sqlite.Migrations;
using System.Data;

namespace CoreCraft.Storage.Sqlite;

/// <summary>
///     A SQLite storage implementation for the domain model
/// </summary>
public sealed class SqliteStorage : IStorage
{
    private readonly MigrationRunner _migrationRunner;
    private readonly IEnumerable<IModelShardStorage> _storages;
    private readonly ISqliteRepositoryFactory _sqliteRepositoryFactory;
    private readonly Action<string>? _logginAction;

    /// <summary>
    ///     Ctor
    /// </summary>
    public SqliteStorage(
        IEnumerable<IMigration> migrations,
        IEnumerable<IModelShardStorage> storages,
        Action<string>? logginAction = null)
        : this(migrations, storages, new SqliteRepositoryFactory(), logginAction)
    {
    }

    internal SqliteStorage(
        IEnumerable<IMigration> migrations,
        IEnumerable<IModelShardStorage> storages,
        ISqliteRepositoryFactory sqliteRepositoryFactory,
        Action<string>? logginAction = null)
    {
        _migrationRunner = new MigrationRunner(migrations);
        _storages = storages;
        _sqliteRepositoryFactory = sqliteRepositoryFactory;
        _logginAction = logginAction;
    }

    /// <inheritdoc cref="IStorage.Update(string, IModelChanges)"/>
    public void Update(string path, IModelChanges changes)
    {
        ISqliteRepository? repository = null;
        IDbTransaction? transaction = null;

        try
        {
            repository = _sqliteRepositoryFactory.Create(path, _logginAction);
            transaction = repository.BeginTransaction();

            foreach (var storage in _storages)
            {
                storage.Update(repository, changes);
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction?.Rollback();
            _logginAction?.Invoke($"Exception was thrown with message: {ex.Message}");
            _logginAction?.Invoke($"Stack trace: {ex.StackTrace}");
            throw;
        }
        finally
        {
            transaction?.Dispose();
            repository?.Dispose();
        }
    }

    /// <inheritdoc cref="IStorage.Save(string, IModel)"/>
    public void Save(string path, IModel model)
    {
        ISqliteRepository? repository = null;
        IDbTransaction? transaction = null;

        try
        {
            repository = _sqliteRepositoryFactory.Create(path, _logginAction);
            transaction = repository.BeginTransaction();

            _migrationRunner.UpdateDatabaseVersion(repository);

            foreach (var storage in _storages)
            {
                storage.Save(repository, model);
            }

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction?.Rollback();
            throw;
        }
        finally
        {
            transaction?.Dispose();
            repository?.Dispose();
        }
    }

    /// <inheritdoc cref="IStorage.Load(string, IModel)"/>
    public void Load(string path, IModel model)
    {
        using var repository = _sqliteRepositoryFactory.Create(path, _logginAction);

        _migrationRunner.Run(repository);

        foreach (var storage in _storages)
        {
            storage.Load(repository, model);
        }
    }
}
