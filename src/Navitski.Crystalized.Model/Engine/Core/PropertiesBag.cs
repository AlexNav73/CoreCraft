using System.Collections;

namespace Navitski.Crystalized.Model.Engine.Core;

/// <inheritdoc cref="IPropertiesBag"/>
public class PropertiesBag : IPropertiesBag
{
    private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();

    /// <inheritdoc />
    public T Read<T>(string name)
    {
        return (T)_properties[name];
    }

    /// <inheritdoc />
    public void Write<T>(string name, T value)
    {
        _properties.Add(name, value!);
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return _properties.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
