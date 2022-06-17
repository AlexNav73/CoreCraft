namespace Navitski.Crystalized.Model.Storage.Sqlite;

/// <summary>
///     Create a new instance of <see cref="ISqliteRepository"/> for a specific file
/// </summary>
public interface ISqliteRepositoryFactory
{
    /// <summary>
    ///     Creates a new instance of a <see cref="ISqliteRepository"/> for a given path
    /// </summary>
    /// <param name="path">A path to the database file</param>
    /// <returns>Repository</returns>
    ISqliteRepository Create(string path);
}
