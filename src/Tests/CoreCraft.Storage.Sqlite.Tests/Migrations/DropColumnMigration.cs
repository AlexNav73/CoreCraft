using CoreCraft.Storage.Sqlite.Migrations;

namespace CoreCraft.Storage.Sqlite.Tests.Migrations;

internal sealed class DropColumnMigration : Migration
{
    private readonly CollectionInfo _table;
    private readonly string _column;

    public DropColumnMigration(CollectionInfo table, string column)
        : base(3)
    {
        _table = table;
        _column = column;
    }

    public override void Migrate(IMigrator migrator)
    {
        migrator.Table(_table).DropColumn(_column);
    }
}
