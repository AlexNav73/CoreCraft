namespace Navitski.Crystalized.Model;

/// <summary>
///     Base implementation of Disposable pattern
/// </summary>
public abstract class DisposableBase : IDisposable
{
    private bool _disposed;

    /// <summary>
    ///     Is object disposed
    /// </summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    ///     Disposes managed resources (called before <see cref="DisposeUnmanagedObjects"/>)
    /// </summary>
    protected virtual void DisposeManagedObjects()
    {
    }

    /// <summary>
    ///     Disposes unmanaged resources (called after <see cref="DisposeManagedObjects"/>)
    /// </summary>
    protected virtual void DisposeUnmanagedObjects()
    {
    }

    /// <summary>
    ///     Last try to cleanup resources
    /// </summary>
    ~DisposableBase()
    {
        Dispose(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            DisposeManagedObjects();
        }

        DisposeUnmanagedObjects();
        _disposed = true;
    }
}
