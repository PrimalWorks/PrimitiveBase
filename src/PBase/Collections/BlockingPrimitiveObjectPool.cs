using System;
using System.Threading;

namespace PBase.Collections
{
    public class BlockingPrimitiveObjectPool<T> : PrimitiveObjectPool<T> where T : class, new()
    {
        #region Private Fields
        private ManualResetEvent m_event;               // Used to block and signal threads instead of throwing exceptions
        private volatile bool m_isCancelled = false;    // Used to make sure threads will unblock when disposing
        #endregion

        #region Constructor
        public BlockingPrimitiveObjectPool(int size) : base(size)
        {
            m_event = new ManualResetEvent(false);
        }
        #endregion

        #region Methods

        #region Overridden Dispose method
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_isCancelled = true;   //Make sure threads don't get caught in while loops
            m_event.Set();          //Unblock all waiting threads
            m_event.Close();
        }
        #endregion

        #region Overridden Obtain and Release methods
        public override T Obtain()
        {
            T item;

            //Multiple threads could be waiting for successful Release
            while (!TryObtain(out item) && !m_isCancelled)
            {
                m_event.Reset();    //Reset so that thread doesn't just fall through WaitOne
                m_event.WaitOne();  //Wait until successful Release
            }

            if (IsDisposing || IsDisposed)
            {
                throw new ObjectDisposedException(nameof(PrimitiveObjectPool<T>));
            }

            m_event.Set();  //Signal that there is at least one item to obtain from pool
            return item;
        }

        public override void Release(T item)
        {
            //Multiple threads could be waiting for successful Obtain
            while (!TryRelease(item) && !m_isCancelled)
            {
                m_event.Reset();    //Reset so that thread doesn't just fall through WaitOne
                m_event.WaitOne();  //Wait until successful Obtain
            }

            if (IsDisposing || IsDisposed)
            {
                throw new ObjectDisposedException(nameof(PrimitiveObjectPool<T>));
            }

            m_event.Set();  //Signal that there is at least one available space to release back into pool
        }
        #endregion

        #endregion
    }
}
