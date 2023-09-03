namespace CoreCraft.Storage.Sqlite.Migrations;

/// <summary>
///     A common interface for the SQLite database migrations
/// </summary>
public interface IMigration
{
    /// <summary>
    ///     Version of a migration
    /// </summary>
    int Version { get; }

    /// <summary>
    ///     Migrates a database to the specific <see cref="Version"/>
    /// </summary>
    /// <param name="migrator">Abstraction over the most common database operations</param>
    void Migrate(IMigrator migrator);
}
