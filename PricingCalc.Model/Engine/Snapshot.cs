using System.Collections;

namespace PricingCalc.Model.Engine;

internal class Snapshot : IModel
{
    private readonly IModel _model;
    private readonly IDictionary<Type, IModelShard> _copies;

    public Snapshot(IModel model)
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
        var copy = ((ICopy<T>)modelShard).Copy();
        _copies.Add(typeof(T), copy);

        return copy;
    }

    /// <summary>
    ///     [Unsafe] Returns model shards without copying if it is not necessary.
    ///     Changes of model shards, returned by this method, won't be tracked!
    ///     (If you want to track changes, use <see cref="GetEnumerator"/> instead)
    /// </summary>
    public IEnumerable<IModelShard> GetShardsInternalUnsafe()
    {
        var shards = _model
            .Where(x => !_copies.Keys.Any(y => y.IsInstanceOfType(x)))
            .Union(_copies.Values);

        return shards;
    }

    public virtual IEnumerator<IModelShard> GetEnumerator()
    {
        var shards = _model
            .Where(x => !_copies.Keys.Any(y => y.IsInstanceOfType(x)));

        foreach (var shard in shards)
        {
            _copies.Add(shard.GetType(), ((ICopy<IModelShard>)shard).Copy());
        }

        return _copies.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
