using System.Diagnostics.CodeAnalysis;

namespace Navitski.Crystalized.Model.Storage.Json.Model;

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

    public void Delete(Guid id)
    {
        var item = Items.SingleOrDefault(x => x.Id == id);
        if (item is not null)
        {
            Items.Remove(item);
        }
        else
        {
            throw new InvalidOperationException($"Unable to delete item with id = [{id}]");
        }
    }
}
