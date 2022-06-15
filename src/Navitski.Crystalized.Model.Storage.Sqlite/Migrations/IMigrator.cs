namespace Navitski.Crystalized.Model.Storage.Sqlite.Migrations;

/// <summary>
///     An abstraction over the most common database operations
/// </summary>
public interface IMigrator
{
    /// <summary>
    ///     Drops a table by the given key
    /// </summary>
    /// <param name="name">Table name</param>
    void DropTable(string name);
}
