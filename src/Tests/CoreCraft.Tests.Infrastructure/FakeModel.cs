using CoreCraft.Engine;
using CoreCraft.Engine.ChangesTracking;
using CoreCraft.Engine.Core;
using CoreCraft.Engine.Scheduling;
using CoreCraft.Engine.Subscription;

namespace CoreCraft.Tests.Infrastructure;

public class FakeModel : DomainModel
{
    public FakeModel(IEnumerable<IModelShard> shards)
        : base(shards, new SyncScheduler())
    {
        UndoStack = new Stack<IModelChanges>();
    }

    public event EventHandler? Changed;

    public Stack<IModelChanges> UndoStack { get; }

    protected override void OnModelChanged(Change<IModelChanges> change)
    {
        UndoStack.Push(change.Hunk);

        Changed?.Invoke(this, EventArgs.Empty);
    }
}
