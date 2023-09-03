using CoreCraft.Storage.Sqlite.Migrations;

namespace CoreCraft.Storage.Sqlite.Tests.Infrastructure;

internal sealed class RenameColumnMigration : Migration
{
    private readonly CollectionInfo _table;
    private readonly string _oldName;
    private readonly string _newName;

    public RenameColumnMigration(CollectionInfo table, string oldName, string newName)
        : base(4)
    {
        _table = table;
        _oldName = oldName;
        _newName = newName;
    }

    public override void Migrate(IMigrator migrator)
    {
        migrator.Table(_table).RenameColumn(_oldName, _newName);
    }
}
