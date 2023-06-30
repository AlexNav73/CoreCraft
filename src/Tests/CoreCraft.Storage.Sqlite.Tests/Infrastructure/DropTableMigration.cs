using CoreCraft.Storage.Sqlite.Migrations;

namespace CoreCraft.Storage.Sqlite.Tests.Infrastructure;

internal class DropTableMigration : Migration
{
    private readonly string _table;

    public DropTableMigration(string table)
        : base(1)
    {
        _table = table;
    }

    public override void Migrate(IMigrator migrator)
    {
        migrator.DropTable(_table);
    }
}
