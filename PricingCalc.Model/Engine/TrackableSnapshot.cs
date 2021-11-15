using PricingCalc.Model.Engine.ChangesTracking;

namespace PricingCalc.Model.Engine;

internal class TrackableSnapshot : Snapshot
{
    public TrackableSnapshot(IModel model) : base(model)
    {
        Changes = new WritableModelChanges();
    }

    public IWritableModelChanges Changes { get; }

    public override T Shard<T>()
    {
        var modelShard = (ITrackableModelShard)base.Shard<T>();
        var trackable = modelShard.AsTrackable(Changes);

        return (T)trackable;
    }

    public override IEnumerator<IModelShard> GetEnumerator()
    {
        var baseEnumerator = base.GetEnumerator();

        IEnumerable<IModelShard> EnumerateTrackables()
        {
            while (baseEnumerator.MoveNext())
            {
                yield return ((ITrackableModelShard)baseEnumerator.Current).AsTrackable(Changes);
            }
        }

        return EnumerateTrackables().GetEnumerator();
    }
}
