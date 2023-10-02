namespace CoreCraft.Storage.Sqlite.Migrations;

internal class SqliteMigrator : IMigrator
{
    private readonly ISqliteRepository _repository;

    public SqliteMigrator(ISqliteRepository repository)
    {
        _repository = repository;
    }

    public ITableOperations Table(string name)
    {
        return new TableOperations(name, _repository);

    }

    public void ExecuteRawSql(string sql)
    {
        _repository.ExecuteNonQuery(sql);
    }
}
