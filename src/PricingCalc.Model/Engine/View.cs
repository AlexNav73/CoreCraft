using PricingCalc.Model.Engine.ChangesTracking;

namespace PricingCalc.Model.Engine;

internal sealed class View
{
    private volatile Model _model;

    public View(IEnumerable<IModelShard> shards)
    {
        _model = new Model(shards);
    }

    public IModel UnsafeModel => _model;

    public TrackableSnapshot CreateTrackableSnapshot()
    {
        return new TrackableSnapshot(_model);
    }

    public Snapshot CreateSnapshot()
    {
        return new Snapshot(_model);
    }

    public IModel CopyModel()
    {
        return new Model(_model.Shards.Select(x => ((ICopy<IModelShard>)x).Copy()));
    }

    public ModelChangeResult ApplySnapshot(Snapshot snapshot, IWritableModelChanges changes)
    {
        var newModel = snapshot.ToModel();
        var oldModel = Interlocked.Exchange(ref _model, newModel);

        return new ModelChangeResult(oldModel, newModel, changes);
    }
}
