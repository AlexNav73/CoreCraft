using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Core;

internal sealed class Snapshot : IModel
{
    private readonly Model _model;
    private readonly IWritableModelChanges? _modelChanges;
    private readonly IDictionary<Type, ICanBeReadOnly<IModelShard>> _copies;

    public Snapshot(Model model, IWritableModelChanges? modelChanges)
    {
        _model = model;
        _modelChanges = modelChanges;
        _copies = new Dictionary<Type, ICanBeReadOnly<IModelShard>>();
    }

    public T Shard<T>() where T : IModelShard
    {
        if (_copies.TryGetValue(typeof(T), out var shard))
        {
            return (T)shard;
        }

        var modelShard = _model.Shard<ICanBeMutable<T>>();
        var mutable = modelShard.AsMutable(_modelChanges);

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
