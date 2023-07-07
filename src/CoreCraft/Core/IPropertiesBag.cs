namespace CoreCraft.Core;

/// <summary>
///     A collection of properties of an entity represented by a property name - value pair
/// </summary>
/// <remarks>
///     Property bags helps to convert any type of properties to the single representation.
///     It is needed for writing data to the database or file when it is not possible to know
///     beforehand which type of properties will be written. If we write the data to the SQLite
///     database, then SQLite provider can determine the type of value (object) and correctly
///     write it to the database, so using an object type here is not a problem. A data also can
///     be red from the properties bag, but converting the property value to a specific type
///     should be done manually (for example using <see cref="Convert.ChangeType(object?, Type)"/>).
/// </remarks>
public interface IPropertiesBag : IEnumerable<KeyValuePair<string, object?>>
{
    /// <summary>
    ///     Writes a property name - value pair to the bag.
    /// </summary>
    /// <typeparam name="T">A type of a value</typeparam>
    /// <param name="name">Name of a property</param>
    /// <param name="value">A value of a property</param>
    void Write<T>(string name, T value);

    /// <summary>
    ///     Reads a property value from the bag by given property name
    /// </summary>
    /// <typeparam name="T">A type of a property value</typeparam>
    /// <param name="name">A name of a property</param>
    /// <returns>A property value</returns>
    T Read<T>(string name);
}
