using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PBase.Collections
{
    public class BlockingPrimitiveObjectPool<T> : PrimitiveObjectPool<T> where T : class, new()
    {
        private AutoResetEvent m_event;

        public BlockingPrimitiveObjectPool(int size) : base(size)
        {
            m_event = new AutoResetEvent(false);
        }
        
        public override T Obtain()
        {
            T item;

            while (!TryObtain(out item))
            {
                m_event.WaitOne();
            }

            m_event.Set();
            return item;
        }

        public override void Release(T item)
        {
            while (!TryRelease(item))
            {
                m_event.WaitOne();
            }

            m_event.Set();
        }
    }
}
