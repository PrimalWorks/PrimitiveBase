using System;
using System.Collections.Concurrent;

namespace PBase.Collections
{
    public class ResizablePrimitiveObjectPool<T> : PrimitiveObjectPool<T> where T : class, new()
    {
        public ResizablePrimitiveObjectPool(int size) : base(size) {}
        
        public override T Obtain()
        {
            if (TryObtain(out T item))
            {
                return item;
            }
            else
            {
                InUse++;
                Size++;
                return new T();
            }
        }

        public override void Release(T item)
        {
            if (!TryRelease(item)) //if pool is full
            {
                m_internalStorage.Add(item);
                Size++;
            }
        }
    }
}
