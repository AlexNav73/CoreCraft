namespace CoreCraft.Persistence;

/// <summary>
///     Represents an interface for objects that can be loaded from a repository.
/// </summary>
public interface ILoadable
{
    /// <summary>
    ///     Loads the object from the specified repository.
    /// </summary>
    /// <param name="repository">The repository from which to load the object.</param>
    void Load(IRepository repository);
}
