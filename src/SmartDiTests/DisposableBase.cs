using System;
namespace SmartDiTests
{
    public abstract class DisposableBase : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
            => Dispose(true);

        protected void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                Disposed = true;

                if (disposing)
                {
                    DisposeExplicit();
                }

                DisposeImplicit();

                GC.SuppressFinalize(this);
            }
        }

        protected virtual void DisposeExplicit() { }
        protected virtual void DisposeImplicit() { }

        ~DisposableBase()
        {
            Dispose(false);
        }
    }
}
