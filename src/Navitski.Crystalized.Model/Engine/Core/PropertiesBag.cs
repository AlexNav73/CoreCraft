using System.Collections;

namespace Navitski.Crystalized.Model.Engine.Core;

public class PropertiesBag : IPropertiesBag
{
    private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();

    public T Read<T>(string name)
    {
        return (T)_properties[name];
    }

    public void Write<T>(string name, T value)
    {
        _properties.Add(name, value!);
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return _properties.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
