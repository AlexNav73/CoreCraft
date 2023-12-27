using CoreCraft.Persistence;
using CoreCraft.Storage.Sqlite.Migrations;
using System.Data;

namespace CoreCraft.Storage.Sqlite;

/// <summary>
///     A SQLite storage implementation for the domain model
/// </summary>
public sealed class SqliteStorage : IStorage
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

    /// <inheritdoc cref="IStorage.Update(IEnumerable{ICanBeSaved})"/>
    public void Update(IEnumerable<ICanBeSaved> modelChanges)
    {
        Transaction(_path, repository =>
        {
            foreach (var change in modelChanges)
            {
                change.Save(repository);
            }
        });
    }

    /// <inheritdoc cref="IStorage.Save(IEnumerable{ICanBeSaved})"/>
    public void Save(IEnumerable<ICanBeSaved> modelShards)
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

    /// <inheritdoc cref="IStorage.Load(IEnumerable{ICanBeLoaded})"/>
    public void Load(IEnumerable<ICanBeLoaded> modelShards)
    {
        using var repository = _sqliteRepositoryFactory.Create(_path, _loggingAction);

        _migrationRunner.Run(repository);

        foreach (var loadable in modelShards)
        {
            loadable.Load(repository);
        }
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
