namespace Navitski.Crystalized.Model.Engine.Core;

internal sealed class Snapshot : IModel
{
    private readonly Model _model;
    private readonly IEnumerable<IFeature> _features;
    private readonly IDictionary<Type, ICanBeReadOnly<IModelShard>> _copies;

    public Snapshot(Model model, IEnumerable<IFeature> features)
    {
        _model = model;
        _features = features;
        _copies = new Dictionary<Type, ICanBeReadOnly<IModelShard>>();
    }

    public T Shard<T>() where T : IModelShard
    {
        if (_copies.TryGetValue(typeof(T), out var shard))
        {
            return (T)shard;
        }

        var modelShard = _model.Shard<ICanBeMutable<T>>();
        var mutable = modelShard.AsMutable(_features);

        _copies.Add(typeof(T), (ICanBeReadOnly<IModelShard>)mutable);

        return mutable;
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
