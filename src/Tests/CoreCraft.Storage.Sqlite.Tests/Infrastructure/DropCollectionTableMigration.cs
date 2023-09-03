using CoreCraft.Storage.Sqlite.Migrations;

namespace CoreCraft.Storage.Sqlite.Tests.Infrastructure;

internal sealed class DropCollectionTableMigration : Migration
{
    private readonly CollectionInfo _table;

    public DropCollectionTableMigration(CollectionInfo table)
        : base(1)
    {
        _table = table;
    }

    public override void Migrate(IMigrator migrator)
    {
        migrator.Table(_table).Drop();
    }
}
