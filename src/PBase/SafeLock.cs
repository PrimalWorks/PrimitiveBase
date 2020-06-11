using System;
using System.Threading;

namespace PBase
{
    public class SafeLock
    {
#if DEBUG
        private static TimeSpan sm_timeout = TimeSpan.FromMinutes(1.0);
#else
        private static TimeSpan sm_timeout = TimeSpan.FromMinutes(10.0);
#endif
        public static TimeSpan Timeout
        {
            get { return sm_timeout; }
            set { sm_timeout = value; }
        }

        private struct SafeLockDisposer : IWaitable
        {
            private SafeLock m_owner;

            public SafeLockDisposer(SafeLock owner)
            {
                m_owner = owner;
            }

            public void Dispose()
            {
                m_owner.Exit();
            }
            
            public bool Wait()
            {
                return m_owner.Wait((int)sm_timeout.TotalMilliseconds);
            }

            public bool Wait(int timeoutMilliseconds)
            {
                return m_owner.Wait(timeoutMilliseconds);
            }

            public void PulseAll()
            {
                m_owner.PulseAll();
            }
        }

        private object m_synchronised = null;

        public SafeLock()
        {
            m_synchronised = new object();
        }

        public bool TryEnter(int timeoutMilliseconds)
        {
            if (Monitor.TryEnter(m_synchronised, timeoutMilliseconds))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryEnter()
        {
            return TryEnter((int)sm_timeout.TotalMilliseconds);
        }

        public IDisposable Enter(int timeoutMilliseconds)
        {
            if (!TryEnter(timeoutMilliseconds))
            {
                throw new TimeoutException("Timed out waiting for lock");
            }

            return new SafeLockDisposer(this);
        }

        public IDisposable Enter()
        {
            return Enter((int)sm_timeout.TotalMilliseconds);
        }
        
        //Wait and PulseAll methods are private
        //so only called from SafeLockDisposer class created when entering lock
        //This ensures these methods are properly used inside a lock
        private bool Wait(int timeoutMilliseconds)
        {
            if (Monitor.Wait(m_synchronised, timeoutMilliseconds))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void PulseAll()
        {
            Monitor.PulseAll(m_synchronised);
        }

        public void Exit()
        {
            Monitor.Exit(m_synchronised);
        }
    }
    
    public interface IWaitable : IDisposable
    {
        bool Wait(int timeoutMilliseconds);
        void PulseAll();
    }
}
