namespace Navitski.Crystalized.Model.Engine;

internal class Snapshot : IModel
{
    private readonly Model _model;
    private readonly IDictionary<Type, IModelShard> _copies;

    public Snapshot(Model model)
    {
        _model = model;
        _copies = new Dictionary<Type, IModelShard>();
    }

    public virtual T Shard<T>() where T : IModelShard
    {
        if (_copies.TryGetValue(typeof(T), out var shard))
        {
            return (T)shard;
        }

        var modelShard = _model.Shard<T>();
        var copy = ((ICopy<IModelShard>)modelShard).Copy();
        _copies.Add(typeof(T), copy);

        return (T)copy;
    }

    public Model ToModel()
    {
        var shards = _model.Shards
            .Where(x => !_copies.Keys.Any(y => y.IsInstanceOfType(x)))
            .Union(_copies.Values);

        return new Model(shards);
    }
}
