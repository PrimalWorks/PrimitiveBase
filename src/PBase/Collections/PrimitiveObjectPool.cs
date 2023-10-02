using System;
using System.Collections.Concurrent;

namespace PBase.Collections
{
    /// <summary>
    /// Default implementation of <c>PrimitiveObjectPool</c>
    /// </summary>
    /// <typeparam name="T"><c>T</c> must be a class with a parameterless constructor</typeparam>
    public class PrimitiveObjectPool<T> : BaseDisposable where T : class, new()
    {
        #region Private/Protected Fields
        private int m_size;                             // Current size of object pool - should always equal bag count + in use count
        private int m_inUse;                            // Number of objects currently in use (i.e., not in pool)    
        protected ConcurrentBag<T> m_internalStorage;   // Storing objects not in use
        #endregion

        #region Public Properties
        /// <summary>
        /// Total size of the object pool - equal to InUse + PoolCount
        /// </summary>
        /// <exception cref="ObjectDisposedException">throws when accessed during or after disposal</exception>
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

        /// <summary>
        /// Number of objects currently used by the application
        /// </summary>
        /// <exception cref="ObjectDisposedException">throws when accessed during or after disposal</exception>
        /// <exception cref="Exception">throws if set to less than 0</exception>
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

        /// <summary>
        /// Number of objects still in the object pool
        /// </summary>
        /// <exception cref="ObjectDisposedException">throws when accessed during or after disposal</exception>
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
        /// <summary>
        /// Create a default <c>PrimitiveObjectPool</c> which holds a number of objects
        /// </summary>
        /// <param name="size">the number of objects this object pool will hold</param>
        /// <exception cref="ArgumentOutOfRangeException">throws if <c>size</c> parameter is less than 1</exception>
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
        /// <summary>
        /// <c>Obtain</c> and remove an object from the pool
        /// </summary>
        /// <returns>returns an object of type <typeparamref name="T"/> from the pool</returns>
        /// <exception cref="InvalidOperationException">throws if there are no more objects in the pool to be obtained</exception>
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

        /// <summary>
        /// <c>TryObtain</c> an object and remove it from the pool - does not throw an exception if pool is empty
        /// </summary>
        /// <param name="item">the object obtained from the pool, if successful - null if unsuccessful</param>
        /// <returns>returns true if successful, false if unsuccessful i.e. pool is empty</returns>
        /// <exception cref="ObjectDisposedException">throws when accessed during or after disposal</exception>
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
        /// <summary>
        /// <c>Release</c> an object and add it back into the pool
        /// </summary>
        /// <param name="item">the object released back into the pool</param>
        /// <exception cref="InvalidOperationException">throws if the pool is full</exception>
        public virtual void Release(T item)
        {
            //Default implementation when releasing on full pool - throw exception
            if (!TryRelease(item))
            {
                throw new InvalidOperationException("Object pool is full");
            }
        }

        /// <summary>
        /// <c>TryRelease</c> an object and add it back into the pool - does not throw exception if pool is full
        /// </summary>
        /// <param name="item">the object released back into the pool</param>
        /// <returns>returns true if successful, false if successful i.e. if pool is full</returns>
        /// <exception cref="ObjectDisposedException">throws when accessed during or after disposal</exception>
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
