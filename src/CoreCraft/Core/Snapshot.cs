namespace CoreCraft.Core;

internal sealed class Snapshot : IMutableModel, ISnapshot
{
    private readonly Model _model;
    private readonly Func<IReadOnlyState<IMutableModelShard>, IMutableModelShard> _converter;
    private readonly IDictionary<Type, IMutableState<IModelShard>> _copies;

    public Snapshot(Model model, Func<IReadOnlyState<IMutableModelShard>, IMutableModelShard> converter)
    {
        _model = model;
        _converter = converter;
        _copies = new Dictionary<Type, IMutableState<IModelShard>>();
    }

    T IModel.Shard<T>()
    {
        if (_copies.TryGetValue(typeof(T), out var shard))
        {
            return (T)shard;
        }

        var modelShard = _model.Shards.OfType<IReadOnlyState<T>>().SingleOrDefault();
        if (modelShard is null)
        {
            throw new InvalidOperationException($"{typeof(T).Name} model shard doesn't implement IReadOnlyState interface");
        }

        var mutable = _converter((IReadOnlyState<IMutableModelShard>)modelShard);

        _copies.Add(typeof(T), (IMutableState<IModelShard>)mutable);

        return (T)mutable;
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
