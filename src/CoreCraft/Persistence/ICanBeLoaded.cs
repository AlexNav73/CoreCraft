namespace CoreCraft.Persistence;

/// <summary>
///     Represents an interface for objects that can be loaded using a specified repository.
/// </summary>
public interface ICanBeLoaded
{
    /// <summary>
    ///     Loads the implementing object using the provided repository.
    /// </summary>
    /// <param name="repository">The repository used to load the object.</param>
    void Load(IRepository repository);
}

