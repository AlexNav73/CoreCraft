using System.Collections;

namespace CoreCraft.Persistence;

internal sealed class LoadSnapshot : ISnapshot, IEnumerable<ICanBeLoaded>
{
    private readonly Model _model;
    private readonly IEnumerable<IFeature> _features;
    private readonly List<ICanBeLoaded> _loaded;
    private readonly List<IModelShard> _cantBeLoaded;

    public LoadSnapshot(Model model, IEnumerable<IFeature> features)
    {
        _model = model;
        _features = features;
        _loaded = new List<ICanBeLoaded>();
        _cantBeLoaded = new List<IModelShard>();
    }

    public Model ToModel()
    {
        var modelShardsAfterLoading = _loaded.Cast<IMutableState<IModelShard>>().Select(x => x.AsReadOnly());

        return new Model(modelShardsAfterLoading.Concat(_cantBeLoaded));
    }

    public IEnumerator<ICanBeLoaded> GetEnumerator()
    {
        foreach (var shard in _model.Shards)
        {
            var mutableShard = ((IReadOnlyState<IModelShard>)shard).AsMutable(_features);
            if (mutableShard is ICanBeLoaded loadable)
            {
                _loaded.Add(loadable);

                yield return loadable;
            }
            else
            {
                _cantBeLoaded.Add(shard);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
