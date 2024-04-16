using CoreCraft.ChangesTracking;
using CoreCraft.Scheduling;
using CoreCraft.Subscription;

namespace CoreCraft;

/// <summary>
///     A domain model which can track all changes happened and provides undo/redo support.
/// </summary>
public class UndoRedoDomainModel : DomainModel
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public UndoRedoDomainModel(
        IEnumerable<IModelShard> modelShards)
        : this(modelShards, new AsyncScheduler())
    {
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    public UndoRedoDomainModel(
        IEnumerable<IModelShard> modelShards,
        IScheduler scheduler)
        : base(modelShards, scheduler)
    {
        History = new ChangesHistory(this);
    }

    /// <summary>
    ///     Provides access to the `ChangesHistory` instance associated with this model.
    /// </summary>
    /// <remarks>
    ///     This <see cref="ChangesHistory"/> object tracks all changes that have happened
    ///     to the model and allows for undo/redo functionality.
    /// </remarks>
    public ChangesHistory History { get; protected set; }

    /// <inheritdoc/>
    protected override void OnModelChanged(Change<IModelChanges> change)
    {
        History.Push(change.Hunk);
    }
}
