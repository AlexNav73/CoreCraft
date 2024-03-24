using System.Diagnostics.CodeAnalysis;
using CoreCraft.Storage.Json.Model.History;

namespace CoreCraft.Storage.Json.Model;

[ExcludeFromCodeCoverage]
internal sealed class Model
{
    public Model()
    {
        ChangesHistory = new List<ModelChanges>();
        Shards = new List<ModelShard>();
    }

    public IList<ModelChanges> ChangesHistory { get; set; }

    public IList<ModelShard> Shards { get; set; }
}
