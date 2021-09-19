using System.Collections.Generic;
using System.Linq;
using PricingCalc.Model.Engine.Persistence;

namespace PricingCalc.Model.Storage.Sqlite
{
    internal sealed class MigrationRunner
    {
        private readonly IEnumerable<IMigration> _migrations;

        public MigrationRunner(IEnumerable<IMigration> migrations)
        {
            _migrations = migrations.OrderBy(x => x.Timestamp);
        }

        public void Run(SqliteRepository repository)
        {
            var pendingMigrations = _migrations;
            IMigration lastMigration = null;

            var result = repository.GetLatestigration();
            if (result != null)
            {
                pendingMigrations = _migrations.Where(x => x.Timestamp > result.Value.timestamp);
            }

            foreach (var migration in pendingMigrations)
            {
                using var transaction = repository.BeginTransaction();
                var migrator = new SqliteMigrator(repository);

                migration.Migrate(migrator);
                lastMigration = migration;

                transaction.Commit();
            }

            if (lastMigration != null)
            {
                repository.UpdateLatestMigration(lastMigration.Timestamp, lastMigration.GetType().Name);
            }
        }

        public void UpdateSaveLatestMigration(SqliteRepository repository)
        {
            var lastMigration = _migrations.LastOrDefault();
            if (lastMigration != null)
            {
                repository.UpdateLatestMigration(lastMigration.Timestamp, lastMigration.GetType().Name);
            }
        }
    }
}
