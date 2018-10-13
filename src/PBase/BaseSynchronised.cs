using System;

namespace PBase
{
    public abstract class BaseSynchronised
    {
        protected SafeLock SyncRoot { get; private set; }

        protected BaseSynchronised()
        {
            SyncRoot = new SafeLock();
        }
    }
}
