using Navitski.Crystalized.Model.Engine.Persistence;

namespace Navitski.Crystalized.Model.Engine.History;

/// <summary>
///     A history which will save all changes happened with the model immediately.
/// </summary>
/// <remarks>
///     Each time model changes, a new change is sent to the history.
///     Current implementation of a history immediately saves the new change.
///     It is automatically subscribes to the model changes and receives
///     them without any additional work to do.
/// </remarks>
public class SaveOnEveryChangeHistory : DisposableBase
{
    private readonly DomainModel _model;
    private readonly IStorage _storage;
    private readonly string _path;

    public SaveOnEveryChangeHistory(DomainModel model, IStorage storage, string path)
    {
        _model = model;
        _storage = storage;
        _path = path;

        _model.ModelChanged += OnModelChanged;
    }

    /// <summary>
    ///     Loads the model
    /// </summary>
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
