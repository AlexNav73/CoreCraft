using System.Data.Common;

namespace Navitski.Crystalized.Model.Storage.Sqlite.Migrations;

internal sealed class MigrationRunner
{
    private readonly IEnumerable<IMigration> _migrations;

    public MigrationRunner(IEnumerable<IMigration> migrations)
    {
        _migrations = migrations.OrderBy(x => x.Version);
    }

    public void Run(SqliteRepository repository)
    {
        var version = repository.GetDatabaseVersion();

        foreach (var migration in _migrations.Where(x => x.Version > version))
        {
            DbTransaction? transaction = null;
            try
            {
                transaction = repository.BeginTransaction();
                var migrator = new SqliteMigrator(repository);

                migration.Migrate(migrator);
                repository.SetDatabaseVersion(migration.Version);

                transaction?.Commit();
            }
            catch (Exception)
            {
                transaction?.Rollback();
                throw;
            }
            finally
            {
                transaction?.Dispose();
            }
        }
    }

    public void UpdateSaveLatestMigration(SqliteRepository repository)
    {
        var lastMigration = _migrations.LastOrDefault();
        if (lastMigration != null)
        {
            repository.SetDatabaseVersion(lastMigration.Version);
        }
    }
}
