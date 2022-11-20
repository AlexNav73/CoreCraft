using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Persistence;
using Navitski.Crystalized.Model.Storage.Sqlite.Migrations;
using System.Data;

namespace Navitski.Crystalized.Model.Storage.Sqlite;

/// <summary>
///     A SQLite storage implementation for the domain model
/// </summary>
public sealed class SqliteStorage : IStorage
{
    private readonly MigrationRunner _migrationRunner;
    private readonly IEnumerable<IModelShardStorage> _storages;
    private readonly ISqliteRepositoryFactory _sqliteRepositoryFactory;

    /// <summary>
    ///     Ctor
    /// </summary>
    public SqliteStorage(
        IEnumerable<IMigration> migrations,
        IEnumerable<IModelShardStorage> storages,
        ISqliteRepositoryFactory sqliteRepositoryFactory)
    {
        _migrationRunner = new MigrationRunner(migrations);
        _storages = storages;
        _sqliteRepositoryFactory = sqliteRepositoryFactory;
    }

    /// <inheritdoc cref="IStorage.Update(string, IModel, IReadOnlyList{IModelChanges})"/>
    public void Update(string path, IModel model, IReadOnlyList<IModelChanges> changes)
    {
        if (changes.Any())
        {
            var merged = (IWritableModelChanges)changes[0];
            for (var i = 1; i < changes.Count; i++)
            {
                merged = merged.Merge(changes[i]);
            }

            if (merged.HasChanges())
            {
                ISqliteRepository? repository = null;
                IDbTransaction? transaction = null;

                try
                {
                    repository = _sqliteRepositoryFactory.Create(path);
                    transaction = repository.BeginTransaction();

                    foreach (var storage in _storages)
                    {
                        storage.Update(repository, model, merged);
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
        }
    }

    /// <inheritdoc cref="IStorage.Save(string, IModel)"/>
    public void Save(string path, IModel model)
    {
        ISqliteRepository? repository = null;
        IDbTransaction? transaction = null;

        try
        {
            repository = _sqliteRepositoryFactory.Create(path);
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
        using var repository = _sqliteRepositoryFactory.Create(path);

        _migrationRunner.Run(repository);

        foreach (var storage in _storages)
        {
            storage.Load(repository, model);
        }
    }
}
