using System.Diagnostics.CodeAnalysis;
using CoreCraft.ChangesTracking;
using CoreCraft.Subscription;

namespace CoreCraft.Views;

/// <summary>
///     Represents an abstract base class for observing and processing changes to a sequence of data frames.
/// </summary>
/// <remarks>
///     Inherit from this class to implement custom logic for handling notifications about changes in a data
///     source. <see cref="DataView{TFrame}"/> provides a framework for observing, processing, and responding to changes, errors, and
///     completion events from an observable sequence. Implementations should override OnNext to define how each change is
///     handled, and may override OnError and OnCompleted to customize error handling and completion behavior. This class
///     manages the subscription lifecycle and ensures proper resource cleanup when disposed.
/// </remarks>
/// <typeparam name="TFrame">
///     The type of the data frame that implements the IChangesFrame interface and represents a single unit of change in the
/// observed sequence.</typeparam>
[ExcludeFromCodeCoverage]
internal abstract class DataView<TFrame> : DisposableBase, IObserver<Change<TFrame>>
    where TFrame : class, IChangesFrame
{
    internal IDisposable? Subscription { get; set; }

    /// <summary>
    /// Notifies the observer that the provider has finished sending push-based notifications.
    /// </summary>
    /// <remarks>After the provider calls OnCompleted, no further notifications will be sent to the observer.
    /// Override this method to perform any finalization or cleanup when the sequence completes.</remarks>
    public virtual void OnCompleted()
    {
    }

    /// <summary>
    /// Handles an error that has occurred during the observable sequence.
    /// </summary>
    /// <remarks>Override this method to provide custom error handling logic when an error is encountered in
    /// the observable sequence. This method is typically called by the observable to notify observers of an
    /// unrecoverable error, after which no further notifications will be sent.</remarks>
    /// <param name="error">The exception that describes the error condition.</param>
    public virtual void OnError(Exception error)
    {
    }

    /// <summary>
    /// Handles a notification that a new change has occurred in the observed sequence.
    /// </summary>
    /// <remarks>Implementations should define how to respond to each change. This method is typically called
    /// by the observable source when a new change is available.</remarks>
    /// <param name="change">The change information to process. Represents the details of the modification to the observed data.</param>
    public abstract void OnNext(Change<TFrame> change);

    /// <inheritdoc />
    protected override void DisposeManagedObjects()
    {
        Subscription?.Dispose();
    }
}
