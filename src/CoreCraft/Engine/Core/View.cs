namespace CoreCraft.Engine.Core;

internal sealed class View
{
    private volatile Model _model;

    public View(IEnumerable<IModelShard> shards)
    {
        _model = new Model(shards);
    }

    public IModel UnsafeModel => _model;

    public Snapshot CreateSnapshot(IEnumerable<IFeature> features)
    {
        return new Snapshot(_model, features);
    }

    public ModelChangeResult ApplySnapshot(Snapshot snapshot)
    {
        var newModel = snapshot.ToModel();
        var oldModel = Interlocked.Exchange(ref _model, newModel);

        return new ModelChangeResult(oldModel, newModel);
    }
}
