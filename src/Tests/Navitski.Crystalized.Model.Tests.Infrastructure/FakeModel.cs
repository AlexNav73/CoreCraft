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
        UndoStack = new Stack<IWritableModelChanges>();
    }

    public event EventHandler? Changed;

    public Stack<IWritableModelChanges> UndoStack { get; }

    protected override void OnModelChanged(Message<IModelChanges> args)
    {
        UndoStack.Push((IWritableModelChanges)args.Changes);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
