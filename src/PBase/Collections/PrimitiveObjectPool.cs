using System;
using System.Collections.Concurrent;

namespace PBase.Collections
{
    public class PrimitiveObjectPool<T> : BaseDisposable where T : class, new()
    {
        public int Size { get; protected set; }

        private int m_inUse;
        public int InUse
        {
            get => m_inUse;
            protected set
            {
                if (value < 0)
                {
                    throw new Exception("Number of in use objects is negative");
                }

                m_inUse = value;
            }
        }

        protected ConcurrentBag<T> m_internalStorage; //Storing objects not in use

        public PrimitiveObjectPool(int size)
        {
            if (size < 1)
            {
                throw new ArgumentOutOfRangeException("size", "size must be greater than one");
            }

            m_internalStorage = new ConcurrentBag<T>();

            for (int i = 0; i < size; i++)
            {
                m_internalStorage.Add(new T());
            }

            Size = size;
        }

        protected override void Dispose(bool disposing)
        {
            Size = 0;
            InUse = 0;
            m_internalStorage = null;
            //need to add checks of isdisposing || isdisposed
        }

        public virtual T Obtain()
        {
            if (TryObtain(out T item))
            {
                return item;
            }
            else
            {
                throw new InvalidOperationException("No more objects to obtain");
            }
        }

        public bool TryObtain(out T item)
        {
            if (m_internalStorage.TryTake(out item))
            {
                InUse++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual void Release(T item)
        {
            if (!TryRelease(item))
            {
                throw new InvalidOperationException("Object pool is full");
            }
        }

        public bool TryRelease(T item)
        {
            if (m_internalStorage.Count == Size)
            {
                return false;
            }
            else
            {
                m_internalStorage.Add(item);
                InUse--;
                return true;
            }
        }
        
        //resizable pool - obtain method automatically increases in size
        //blocking pool - blocks until succ release
        //default (implemented here) just throws if no objects to obtain from bag
    }
}
