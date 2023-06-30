using System.Data;

namespace CoreCraft.Storage.Sqlite.Migrations;

internal sealed class MigrationRunner
{
    private readonly IReadOnlyList<IMigration> _migrations;

    public MigrationRunner(IEnumerable<IMigration> migrations)
    {
        _migrations = migrations.OrderBy(x => x.Version).ToArray();
    }

    public void Run(ISqliteRepository repository)
    {
        var version = repository.GetDatabaseVersion();

        foreach (var migration in _migrations.Where(x => x.Version > version))
        {
            IDbTransaction? transaction = null;
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

    public void UpdateDatabaseVersion(ISqliteRepository repository)
    {
        if (_migrations.Any())
        {
            repository.SetDatabaseVersion(_migrations[^1].Version);
        }
    }
}
