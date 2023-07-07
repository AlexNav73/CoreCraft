using System.Collections;

namespace CoreCraft.Core;

/// <inheritdoc cref="IPropertiesBag"/>
public sealed class PropertiesBag : IPropertiesBag
{
    private readonly IDictionary<string, object?> _properties = new Dictionary<string, object?>();

    /// <inheritdoc />
    public T Read<T>(string name)
    {
        return (T)_properties[name]!;
    }

    /// <inheritdoc />
    public void Write<T>(string name, T value)
    {
        _properties.Add(name, value);
    }

    /// <summary>
    ///     Compares two sets of properties and returns a new set with properties
    ///     which are different
    /// </summary>
    /// <param name="newProperies">A bag with new properties</param>
    /// <returns>A new bag with properties which were modified</returns>
    public PropertiesBag Compare(PropertiesBag newProperies)
    {
        var result = new PropertiesBag();

        foreach (var pair in this)
        {
            var newProp = newProperies._properties[pair.Key];
            if ((pair.Value != null && !pair.Value.Equals(newProp))
                || (pair.Value == null && newProp != null))
            {
                result._properties.Add(pair.Key, newProp);
            }
        }

        return result;
    }

    /// <summary>
    ///     Checks weather a properties bag contains a property with a given name
    /// </summary>
    /// <param name="name">A property name</param>
    /// <returns>True - if properties bag contains property</returns>
    public bool ContainsProp(string name)
    {
        return _properties.ContainsKey(name);
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        return _properties.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
