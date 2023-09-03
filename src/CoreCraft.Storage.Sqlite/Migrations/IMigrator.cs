using CoreCraft.Core;

namespace CoreCraft.Storage.Sqlite.Migrations;

/// <summary>
///     An abstraction over the most common database operations
/// </summary>
public interface IMigrator
{
    /// <summary>
    ///     Gets table operations for the specified collection.
    /// </summary>
    /// <param name="collection">The collection information for which table operations are needed.</param>
    /// <returns>An instance of table operations for the specified collection.</returns>
    ICollectionTableOperations Table(CollectionInfo collection);

    /// <summary>
    ///     Gets table operations for the specified relation.
    /// </summary>
    /// <param name="relation">The relation information for which table operations are needed.</param>
    /// <returns>An instance of table operations for the specified relation.</returns>
    ITableOperations Table(RelationInfo relation);

    /// <summary>
    ///     Executes raw SQL commands on the database.
    /// </summary>
    /// <param name="sql">The SQL command to execute.</param>
    void ExecuteRawSql(string sql);
}
