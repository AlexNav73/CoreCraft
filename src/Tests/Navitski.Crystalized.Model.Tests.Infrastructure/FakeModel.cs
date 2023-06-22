using Navitski.Crystalized.Model.Engine;
using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Core;
using Navitski.Crystalized.Model.Engine.Scheduling;
using Navitski.Crystalized.Model.Engine.Subscription;

namespace Navitski.Crystalized.Model.Tests.Infrastructure;

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
