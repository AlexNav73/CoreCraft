namespace CoreCraft.Core;

internal sealed class View
{
    private volatile Model _model;

    public View(IEnumerable<IModelShard> shards)
    {
        _model = new Model(shards);
    }

    public Model UnsafeModel => _model;

    public ModelChangeResult ApplySnapshot(ISnapshot snapshot)
    {
        var newModel = snapshot.ToModel();
        var oldModel = Interlocked.Exchange(ref _model, newModel);

        return new ModelChangeResult(oldModel, newModel);
    }
}
