using Navitski.Crystalized.Model.Engine.Persistence;

namespace Navitski.Crystalized.Model.Engine.History;

internal class SaveOnEveryChangeHistory : DisposableBase
{
    private readonly BaseModel _model;
    private readonly IStorage _storage;
    private readonly string _path;

    public SaveOnEveryChangeHistory(BaseModel model, IStorage storage, string path)
    {
        _model = model;
        _storage = storage;
        _path = path;

        _model.ModelChanged += OnModelChanged;
    }

    public async Task Load()
    {
        await _model.Load(_storage, _path);
    }

    internal async void OnModelChanged(object? sender, ModelChangedEventArgs args)
    {
        // TODO(#10): Saving performed in the thread pool's thread and if some
        // sequential changes come, saving order of thees changes is unpredictable.
        // Currently, changes' frequency is low and every change have enough time
        // for saving. In future, changes could be more frequent and it is necessary
        // to queue changes for saving or batch them in one big change
        await _model.Save(_storage, _path, new[] { args.Changes });
    }

    protected override void DisposeManagedObjects()
    {
        _model.ModelChanged -= OnModelChanged;
    }
}
