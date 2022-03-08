using PricingCalc.Model.Engine.ChangesTracking;

namespace PricingCalc.Model.Engine;

internal class TrackableSnapshot : Snapshot
{
    public TrackableSnapshot(Model model) : base(model)
    {
        Changes = new WritableModelChanges();
    }

    public IWritableModelChanges Changes { get; }

    public override T Shard<T>()
    {
        var modelShard = base.Shard<ITrackableModelShard<T>>();
        var trackable = modelShard.AsTrackable(Changes);

        return trackable;
    }
}
