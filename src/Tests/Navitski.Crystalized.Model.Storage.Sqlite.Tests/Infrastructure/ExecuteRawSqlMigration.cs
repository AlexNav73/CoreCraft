using Navitski.Crystalized.Model.Storage.Sqlite.Migrations;

namespace Navitski.Crystalized.Model.Storage.Sqlite.Tests.Infrastructure;

internal class ExecuteRawSqlMigration : Migration
{
    private readonly string _sql;

    public ExecuteRawSqlMigration(string sql)
        : base(1)
    {
        _sql = sql;
    }

    public override void Migrate(IMigrator migrator)
    {
        migrator.ExecuteRawSql(_sql);
    }
}
