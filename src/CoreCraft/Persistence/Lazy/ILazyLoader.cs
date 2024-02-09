namespace CoreCraft.Persistence.Lazy;

/// <summary>
///     Represents an interface for objects that support lazy loading from a repository.
/// </summary>
public interface ILazyLoader
{
    /// <summary>
    ///     Loads the object lazily from the specified repository.
    /// </summary>
    /// <param name="repository">The repository from which to lazily load the object.</param>
    void Load(IRepository repository);
}
