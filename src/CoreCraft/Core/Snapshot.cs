namespace CoreCraft.Core;

internal sealed class Snapshot : IMutableModel, ISnapshot
{
    private readonly Model _model;
    private readonly IEnumerable<IFeature> _features;
    private readonly IDictionary<Type, IMutableState<IModelShard>> _copies;

    public Snapshot(Model model, IEnumerable<IFeature> features)
    {
        _model = model;
        _features = features;
        _copies = new Dictionary<Type, IMutableState<IModelShard>>();
    }

    T IModel.Shard<T>()
    {
        if (_copies.TryGetValue(typeof(T), out var shard))
        {
            return (T)shard;
        }

        var modelShard = _model.Shards.OfType<IReadOnlyState<T>>().Single();
        var mutable = modelShard.AsMutable(_features);

        _copies.Add(typeof(T), (IMutableState<IModelShard>)mutable);

        return mutable;
    }

    T IMutableModel.Shard<T>()
    {
        return ((IModel)this).Shard<T>();
    }

    public Model ToModel()
    {
        var readOnlyCopies = _copies.Values.Select(x => x.AsReadOnly()).ToArray();

        var shards = _model.Shards
            .Where(x => readOnlyCopies.All(y => y.GetType() != x.GetType()))
            .Union(readOnlyCopies);

        return new Model(shards);
    }
}
