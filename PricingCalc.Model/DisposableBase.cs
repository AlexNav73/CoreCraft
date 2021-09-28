using System;

namespace PricingCalc.Model
{
    public abstract class DisposableBase : IDisposable
    {
        private bool _disposed;

        public bool IsDisposed => _disposed;

        protected virtual void DisposeManagedObjects()
        {
        }

        protected virtual void DisposeUnmanagedObjects()
        {
        }

        ~DisposableBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
}
