using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Storage.Json.Model;

[ExcludeFromCodeCoverage]
internal sealed class Relation
{
    public Relation(string name)
    {
        Name = name;

        Pairs = new List<Pair>();
    }

    public string Name { get; set; }

    public IList<Pair> Pairs { get; set; }
}
