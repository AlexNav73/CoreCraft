namespace Navitski.Crystalized.Model.Storage.Sqlite.Migrations;

public interface IMigration
{
    long Version { get; }

    void Migrate(IMigrator migrator);
}
