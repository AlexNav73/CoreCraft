namespace Navitski.Crystalized.Model.Storage.Sqlite.Migrations;

public abstract class Migration : IMigration
{
    protected Migration(long timestamp)
    {
        Timestamp = timestamp;
    }

    public long Timestamp { get; }

    public abstract void Migrate(IMigrator migrator);
}
