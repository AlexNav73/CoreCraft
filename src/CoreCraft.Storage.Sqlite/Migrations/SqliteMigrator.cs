using CoreCraft.Core;

namespace CoreCraft.Storage.Sqlite.Migrations;

internal class SqliteMigrator : IMigrator
{
    private readonly ISqliteRepository _repository;

    public SqliteMigrator(ISqliteRepository repository)
    {
        _repository = repository;
    }

    public ICollectionTableOperations Table(CollectionInfo collection)
    {
        return new TableOperations(QueryBuilder.InferName(collection), _repository);
    }

    public ITableOperations Table(RelationInfo relation)
    {
        return new TableOperations(QueryBuilder.InferName(relation), _repository);

    }

    public void ExecuteRawSql(string sql)
    {
        _repository.ExecuteNonQuery(sql);
    }
}
