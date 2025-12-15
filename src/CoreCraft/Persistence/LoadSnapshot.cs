using System.Collections;
using CoreCraft.ChangesTracking;

namespace CoreCraft.Persistence;

internal sealed class LoadSnapshot : ISnapshot, IEnumerable<IMutableModelShard>
{
    private readonly IReadOnlyCollection<IMutableModelShard> _mutableModelShards;

    public LoadSnapshot(Model model, IMutableModelChanges changes)
    {
        _mutableModelShards = model.Shards
            .Select(s => ((IReadOnlyState<IMutableModelShard>)s).AsLoadModel(changes))
            .ToArray();
    }

    public Model ToModel()
    {
        var modelShardsAfterLoading = _mutableModelShards
            .Cast<IMutableState<IModelShard>>()
            .Select(x => x.AsReadOnly());

        return new Model(modelShardsAfterLoading);
    }

    public IEnumerator<IMutableModelShard> GetEnumerator()
    {
        return _mutableModelShards.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _mutableModelShards.GetEnumerator();
    }
}
