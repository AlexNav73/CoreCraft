using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Storage.Json.Model;

[ExcludeFromCodeCoverage]
internal sealed class Collection<T> : ICollection
{
    public Collection(string name)
    {
        Name = name;

        Items = new List<Item<T>>();
    }

    public string Name { get; set; }

    public IList<Item<T>> Items { get; set; }
}
