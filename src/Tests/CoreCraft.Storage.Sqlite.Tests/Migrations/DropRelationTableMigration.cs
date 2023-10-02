using CoreCraft.Storage.Sqlite.Migrations;

namespace CoreCraft.Storage.Sqlite.Tests.Migrations;

internal sealed class DropRelationTableMigration : Migration
{
    private readonly RelationInfo _table;

    public DropRelationTableMigration(RelationInfo table)
        : base(1)
    {
        _table = table;
    }

    public override void Migrate(IMigrator migrator)
    {
        migrator.Table(NameOf(_table)).Drop();
    }
}
