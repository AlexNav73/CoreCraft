using System.Collections.Generic;
using System.IO;
using PricingCalc.Model.Engine;
using PricingCalc.Model.Engine.ChangesTracking;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.Storage.Sqlite
{
    public sealed class SqliteModelStorage : IStorage
    {
        private readonly MigrationRunner _migrationRunner;

        public SqliteModelStorage(IEnumerable<IMigration> migrations)
        {
            _migrationRunner = new MigrationRunner(migrations);
        }

        public void Save(string path, IModel model, IReadOnlyList<IModelChanges> changes)
        {
            using var repository = new SqliteRepository(path);
            using var transaction = repository.BeginTransaction();

            for (var i = 0; i < changes.Count; i++)
            {
                foreach (var modelShard in model)
                {
                    ((IHaveStorage)modelShard).Storage.Save(path, repository, changes[i]);
                }
            }

            transaction.Commit();
        }

        public void Save(string path, IModel model)
        {
            using var repository = new SqliteRepository(path);
            using var transaction = repository.BeginTransaction();

            _migrationRunner.UpdateSaveLatestMigration(repository);

            foreach (var modelShard in model)
            {
                ((IHaveStorage)modelShard).Storage.Save(path, repository);
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

            foreach (var modelShard in model)
            {
                ((IHaveStorage)modelShard).Storage.Load(path, repository);
            }
        }
    }
}
