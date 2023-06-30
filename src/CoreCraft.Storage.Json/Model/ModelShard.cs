using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Storage.Json.Model;

[ExcludeFromCodeCoverage]
internal sealed class ModelShard
{
    public ModelShard(string name)
    {
        Name = name;

        Collections = new List<ICollection>();
        Relations = new List<Relation>();
    }

    public string Name { get; set; }

    public IList<ICollection> Collections { get; set; }

    public IList<Relation> Relations { get; set; }
}
