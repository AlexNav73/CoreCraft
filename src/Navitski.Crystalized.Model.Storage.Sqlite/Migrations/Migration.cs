namespace Navitski.Crystalized.Model.Storage.Sqlite.Migrations;

public abstract class Migration : IMigration
{
    protected Migration(long version)
    {
        Version = version;
    }

    public long Version { get; }

    public abstract void Migrate(IMigrator migrator);
}
