using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Storage.Sqlite.Migrations;
using System.Data.Common;

namespace Navitski.Crystalized.Model.Storage.Sqlite;

/// <summary>
///     A SQLite storage implementation for the domain model
/// </summary>
public sealed class SqliteStorage : IStorage
{
    private readonly MigrationRunner _migrationRunner;
    private readonly IEnumerable<IModelShardStorage> _storages;

    /// <summary>
    ///     Ctor
    /// </summary>
    public SqliteStorage(
        IEnumerable<IMigration> migrations,
        IEnumerable<IModelShardStorage> storages)
    {
        _migrationRunner = new MigrationRunner(migrations);
        _storages = storages;
    }

    /// <inheritdoc cref="IStorage.Migrate(string, IModel, IReadOnlyList{IModelChanges})"/>
    public void Migrate(string path, IModel model, IReadOnlyList<IModelChanges> changes)
    {
        SqliteRepository? repository = null;
        DbTransaction? transaction = null;

        try
        {
            repository = new SqliteRepository(path);
            transaction = repository.BeginTransaction();

            for (var i = 0; i < changes.Count; i++)
            {
                foreach (var storage in _storages)
                {
                    storage.Migrate(repository, model, changes[i]);
                }
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

    /// <inheritdoc cref="IStorage.Save(string, IModel)"/>
    public void Save(string path, IModel model)
    {
        SqliteRepository? repository = null;
        DbTransaction? transaction = null;

        try
        {
            repository = new SqliteRepository(path);
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
        if (!File.Exists(path))
        {
            return;
        }

        using var repository = new SqliteRepository(path);

        _migrationRunner.Run(repository);

        foreach (var storage in _storages)
        {
            storage.Load(repository, model);
        }
    }
}
