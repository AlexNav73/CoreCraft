using CoreCraft.Storage.Sqlite.Migrations;

namespace CoreCraft.Storage.Sqlite.Tests.Migrations;

internal sealed class ColumnWithUnsupportedTypeMigration : Migration
{
    private readonly CollectionInfo _table;
    private readonly string _name;

    public ColumnWithUnsupportedTypeMigration(CollectionInfo table, string name)
        : base(1)
    {
        _table = table;
        _name = name;
    }

    public override void Migrate(IMigrator migrator)
    {
        migrator.Table(NameOf(_table)).AddColumn<ColumnWithUnsupportedTypeMigration>(_name, false);
    }
}
