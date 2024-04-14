using CoreCraft.Storage.Sqlite.Migrations;

namespace CoreCraft.Storage.Sqlite.Tests.Migrations;

internal class FailingMigration : Migration
{
    public FailingMigration()
        : base(1)
    {
    }

    public override void Migrate(IMigrator migrator)
    {
        throw new NotImplementedException();
    }
}
