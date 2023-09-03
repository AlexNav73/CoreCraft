namespace CoreCraft.Storage.Sqlite.Migrations;

/// <summary>
///     Represents a set of operations that can be performed on a collection table.
/// </summary>
public interface ICollectionTableOperations : ITableOperations
{
    /// <summary>
    ///     Adds a new column to the database table.
    /// </summary>
    /// <typeparam name="TColumn">The data type of the column.</typeparam>
    /// <param name="name">The name of the new column.</param>
    /// <param name="isNullable">The nullability flag for the new column.</param>
    /// <param name="defaultValue">The default value for the new column (optional).</param>
    void AddColumn<TColumn>(string name, bool isNullable, TColumn? defaultValue = default);

    /// <summary>
    ///     Drops a column from the database table.
    /// </summary>
    /// <param name="name">The name of the column to drop.</param>
    void DropColumn(string name);

    /// <summary>
    ///     Renames a column in the database table.
    /// </summary>
    /// <param name="oldName">The current name of the column to be renamed.</param>
    /// <param name="newName">The new name for the column.</param>
    void RenameColumn(string oldName, string newName);
}
