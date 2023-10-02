using CoreCraft.Core;

namespace CoreCraft.Storage.Sqlite.Migrations;

internal sealed class TableOperations : ITableOperations
{
    private readonly string _table;
    private readonly ISqliteRepository _repository;

    public TableOperations(string table, ISqliteRepository repository)
    {
        _table = table;
        _repository = repository;
    }

    public void Drop()
    {
        _repository.ExecuteNonQuery(QueryBuilder.DropTable(_table));
    }

    public void AddColumn<TColumn>(string name, bool isNullable, TColumn? defaultValue = default)
    {
        var type = typeof(TColumn);
        var columnType = Nullable.GetUnderlyingType(type);
        var column = new PropertyInfo(name, columnType ?? type, isNullable);
        var query = QueryBuilder.AddColumn(_table, column, QuoteIfString(column, defaultValue));

        _repository.ExecuteNonQuery(query);
    }

    public void DropColumn(string name)
    {
        _repository.ExecuteNonQuery(QueryBuilder.DropColumn(_table, name));
    }

    public void RenameColumn(string oldName, string newName)
    {
        _repository.ExecuteNonQuery(QueryBuilder.RenameColumn(_table, oldName, newName));
    }

    private string? QuoteIfString<T>(PropertyInfo column, T? value)
    {
        SqlTypeMapper.EnsureSupported(column.Type);

        if (column.Type == typeof(string))
        {
            if (value is not null)
            {
                return $"\"{value}\"";
            }
            else if (!column.IsNullable)
            {
                return "\"\"";
            }
        }

        return value?.ToString();
    }
}
