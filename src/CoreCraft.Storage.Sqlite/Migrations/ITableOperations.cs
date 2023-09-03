namespace CoreCraft.Storage.Sqlite.Migrations;

/// <summary>
///     Represents a set of operations that can be performed on a database table.
/// </summary>
public interface ITableOperations
{
    /// <summary>
    ///     Drops the database table.
    /// </summary>
    void Drop();
}
