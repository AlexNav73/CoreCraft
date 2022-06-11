namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     A base type to represent properties of an entity
/// </summary>
/// <remarks>
///     There is a separation between a entity and it's properties. It is needed
///     for the modification of the data. An object can hold a reference to an entity
///     and query the latest version of properties at any time. Using this approach
///     an object can be sure that it has the most recent properties.
/// </remarks>
public abstract record Properties
{
    /// <summary>
    ///     Writes properties to a <see cref="IPropertiesBag"/>
    /// </summary>
    /// <param name="bag">A properties bag</param>
    public abstract void WriteTo(IPropertiesBag bag);

    /// <summary>
    ///     Reads properties from the <see cref="IPropertiesBag"/>
    /// </summary>
    /// <param name="bag">A properties bag</param>
    /// <returns>Properties</returns>
    public abstract Properties ReadFrom(IPropertiesBag bag);
}
