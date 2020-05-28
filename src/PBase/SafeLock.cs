using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
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

        private struct SafeLockDisposer : IDisposable
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
        }

        private object m_synchronised = null;
        private Thread m_thread = null;
        private int m_ref = 0;

        public SafeLock()
        {
            m_synchronised = new object();
        }

        public bool TryEnter(int timeoutMilliseconds)
        {
            if (Monitor.TryEnter(m_synchronised, timeoutMilliseconds))
            {
                if (m_thread == null)
                {
                    m_thread = Thread.CurrentThread;
                    m_ref = 1;
                }
                else if (m_thread == Thread.CurrentThread)
                {
                    m_ref++;
                }
                else
                {
                    Monitor.Exit(m_synchronised);
                    throw new Exception("SafeLock held by another Thread");
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryEnter()
        {
            return TryEnter(sm_timeout.Milliseconds);
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
            return Enter(sm_timeout.Milliseconds);
        }

        /*public bool Wait(int timeoutMilliseconds = -1)
        {


            //return Monitor.Wait(m_synchronised, timeoutMilliseconds);
        }*/

        public void Exit()
        {
            if (m_thread != Thread.CurrentThread)
            {
                throw new SafeLockException("Only the locks thread can release it");
            }

            m_ref--;

            if (m_ref == 0)
            {
                m_thread = null;
            }

            Monitor.Exit(m_synchronised);
        }
    }

    public class SafeLockException : Exception
    {
        public SafeLockException(string message) : base(message)
        {
        }
    }
}
