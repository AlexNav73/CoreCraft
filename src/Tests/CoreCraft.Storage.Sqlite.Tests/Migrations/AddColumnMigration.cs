using CoreCraft.Storage.Sqlite.Migrations;

namespace CoreCraft.Storage.Sqlite.Tests.Migrations;

internal sealed class AddColumnMigration<T> : Migration
{
    private readonly CollectionInfo _table;
    private readonly string _column;
    private readonly bool _isNullable;
    private readonly T? _defaultValue;

    public AddColumnMigration(CollectionInfo table, string column, bool isNullable, T? defaultValue = default)
        : base(2)
    {
        _table = table;
        _column = column;
        _isNullable = isNullable;
        _defaultValue = defaultValue;
    }

    public override void Migrate(IMigrator migrator)
    {
        migrator.Table(NameOf(_table)).AddColumn(_column, _isNullable, _defaultValue);
    }
}
