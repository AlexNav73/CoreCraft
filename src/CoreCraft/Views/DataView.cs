using System.Diagnostics.CodeAnalysis;
using CoreCraft.ChangesTracking;
using CoreCraft.Subscription;

namespace CoreCraft.Views;

[ExcludeFromCodeCoverage]
internal abstract class DataView<TFrame> : DisposableBase, IObserver<Change<TFrame>>
    where TFrame : class, IChangesFrame
{
    internal IDisposable? Subscription { get; set; }

    public virtual void OnCompleted()
    {
    }

    public virtual void OnError(Exception error)
    {
    }

    public abstract void OnNext(Change<TFrame> change);

    protected override void DisposeManagedObjects()
    {
        Subscription?.Dispose();
    }
}
