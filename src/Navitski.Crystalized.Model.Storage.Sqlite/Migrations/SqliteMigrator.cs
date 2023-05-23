namespace Navitski.Crystalized.Model.Storage.Sqlite.Migrations;

internal class SqliteMigrator : IMigrator
{
    private readonly ISqliteRepository _repository;

    public SqliteMigrator(ISqliteRepository repository)
    {
        _repository = repository;
    }

    public void DropTable(string name)
    {
        _repository.ExecuteNonQuery(QueryBuilder.DropTable(name));
    }

    public void ExecuteRawSql(string sql)
    {
        _repository.ExecuteNonQuery(sql);
    }
}
