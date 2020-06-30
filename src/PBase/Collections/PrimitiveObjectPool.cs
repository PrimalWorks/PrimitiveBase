using System;
using System.Collections.Concurrent;

namespace PBase.Collections
{
    public class PrimitiveObjectPool<T> : BaseDisposable where T : class, new()
    {
        #region Private/Protected Fields
        private int m_size;                             // Current size of object pool - should always equal bag count + in use count
        private int m_inUse;                            // Number of objects currently in use (i.e., not in pool)    
        protected ConcurrentBag<T> m_internalStorage;   // Storing objects not in use
        #endregion

        #region Public Properties
        public int Size
        {
            get
            {
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(PrimitiveObjectPool<T>));
                }

                return m_size;
            }

            protected set
            {
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(PrimitiveObjectPool<T>));
                }

                m_size = value;
            }
        }

        public int InUse
        {
            get
            {
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(PrimitiveObjectPool<T>));
                }

                return m_inUse;
            }

            protected set
            {
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(PrimitiveObjectPool<T>));
                }

                if (value < 0)
                {
                    throw new Exception("Number of in use objects is negative");
                }

                m_inUse = value;
            }
        }

        public int PoolCount
        {
            get
            {
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(PrimitiveObjectPool<T>));
                }

                return m_internalStorage.Count;
            }
        }
        #endregion

        #region Constructor
        public PrimitiveObjectPool(int size)
        {
            if (size < 1)
            {
                throw new ArgumentOutOfRangeException("size", "size must be greater than one");
            }

            m_internalStorage = new ConcurrentBag<T>();

            for (int i = 0; i < size; i++)
            {
                //Instantiate with default constructor
                m_internalStorage.Add(new T());
            }

            Size = size;
        }
        #endregion

        #region Methods

        #region Dispose Method
        protected override void Dispose(bool disposing)
        {
            m_size = 0;
            m_inUse = 0;
            m_internalStorage = null;
        }
        #endregion

        #region Obtain Methods
        public virtual T Obtain()
        {
            if (TryObtain(out T item))
            {
                return item;
            }
            else //Default implementation when obtaining on empty pool - throw exception
            {
                throw new InvalidOperationException("No more objects to obtain");
            }
        }

        public bool TryObtain(out T item)
        {
            using (SyncRoot.Enter())
            {
                //This is called from Obtain so only need to check dispose state here
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(PrimitiveObjectPool<T>));
                }

                if (m_internalStorage.TryTake(out item)) //This will decrement count
                {
                    InUse++;
                    return true;
                }
                else //if bag is empty
                {
                    return false;
                }
            }
        }
        #endregion

        #region Release Methods
        public virtual void Release(T item)
        {
            //Default implementation when releasing on full pool - throw exception
            if (!TryRelease(item))
            {
                throw new InvalidOperationException("Object pool is full");
            }
        }

        public bool TryRelease(T item)
        {
            using (SyncRoot.Enter())
            {
                //This is called from Obtain so only need to check dispose state here
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(PrimitiveObjectPool<T>));
                }

                //If storage bag is full i.e., equal to initial size, don't release item into bag
                if (m_internalStorage.Count == Size)
                {
                    return false;
                }
                else
                {
                    m_internalStorage.Add(item); //This will increment storage bag count
                    InUse--;
                    return true;
                }
            }
        }
        #endregion

        #endregion
    }
}
