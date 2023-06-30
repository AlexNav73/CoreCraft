using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Storage.Json.Model;

[ExcludeFromCodeCoverage]
internal sealed class Item<T>
{
    public Item(Guid id, T properties)
    {
        Id = id;
        Properties = properties;
    }

    public Guid Id { get; }

    public T Properties { get; set; }
}
