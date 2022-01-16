using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.ChangesTracking;
using PricingCalc.Model.Engine.Persistence;
using PricingCalc.Model.Storage.Sqlite.Migrations;

namespace PricingCalc.Model.Storage.Sqlite;

public sealed class SqliteModelStorage : IStorage
{
    private readonly MigrationRunner _migrationRunner;
    private readonly IEnumerable<IModelShardStorage> _storages;

    public SqliteModelStorage(
        IEnumerable<IMigration> migrations,
        IEnumerable<IModelShardStorage> storages)
    {
        _migrationRunner = new MigrationRunner(migrations);
        _storages = storages;
    }

    public void Save(string path, IModel model, IReadOnlyList<IModelChanges> changes)
    {
        using var repository = new SqliteRepository(path);
        using var transaction = repository.BeginTransaction();

        for (var i = 0; i < changes.Count; i++)
        {
            foreach (var storage in _storages)
            {
                storage.Save(repository, model, changes[i]);
            }
        }

        transaction.Commit();
    }

    public void Save(string path, IModel model)
    {
        using var repository = new SqliteRepository(path);
        using var transaction = repository.BeginTransaction();

        _migrationRunner.UpdateSaveLatestMigration(repository);

        foreach (var storage in _storages)
        {
            storage.Save(repository, model);
        }

        transaction.Commit();
    }

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
