namespace CoreCraft.Storage.Sqlite.Migrations;

/// <summary>
///     An abstraction over the most common database operations
/// </summary>
public interface IMigrator
{
    /// <summary>
    ///     Gets table operations for the specified relation.
    /// </summary>
    /// <param name="name">The table name for which operations are needed.</param>
    /// <returns>An instance of table operations for the specified relation.</returns>
    ITableOperations Table(string name);

    /// <summary>
    ///     Executes raw SQL commands on the database.
    /// </summary>
    /// <param name="sql">The SQL command to execute.</param>
    void ExecuteRawSql(string sql);
}
