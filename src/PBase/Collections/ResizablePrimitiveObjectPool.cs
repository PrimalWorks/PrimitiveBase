using System;
using System.Collections.Concurrent;

namespace PBase.Collections
{
    /// <summary>
    /// <c>ResizeablePrimitiveObjectPool</c> increases pool size when obtaining on empty pool and releasing on full pool
    /// </summary>
    /// <typeparam name="T"><c>T</c> must be a class with a parameterless constructor</typeparam>
    public class ResizablePrimitiveObjectPool<T> : PrimitiveObjectPool<T> where T : class, new()
    {
        #region Constructor
        /// <summary>
        /// Create a <c>ResizeablePrimitiveObjectPool</c> that increase pool size instead of throwing an <c>ArgumentOutOfRangeException</c>
        /// </summary>
        /// <param name="size">the number of objects this object pool will initially hold</param>
        /// <exception cref="ArgumentOutOfRangeException">throws if <c>size</c> parameter is less than 1</exception>
        public ResizablePrimitiveObjectPool(int size) : base(size) {}
        #endregion

        #region Overridden Obtain and Release methods
        /// <summary>
        /// <c>Obtain</c> and remove an object from the pool - increases pool size if the pool is empty
        /// </summary>
        /// <returns>returns an object of type <typeparamref name="T"/> from the pool</returns>
        /// <exception cref="ObjectDisposedException">throws when accessed during or after disposal</exception>
        public override T Obtain()
        {
            if (TryObtain(out T item))
            {
                return item;
            }
            else //If the pool is empty
            {
                using (SyncRoot.Enter())
                {
                    if (IsDisposing || IsDisposed)
                    {
                        throw new ObjectDisposedException(nameof(PrimitiveObjectPool<T>));
                    }

                    InUse++;
                    Size++;         //Increment Size so releasing item will just add to pool in TryRelease
                    return new T(); //Create new item
                }
            }
        }

        /// <summary>
        /// <c>Release</c> an object and add it back into the pool - increases pool size if the pool is full
        /// </summary>
        /// <param name="item">the object released back into the pool</param>
        /// <exception cref="ObjectDisposedException">throws when accessed during or after disposal</exception>
        public override void Release(T item)
        {
            if (!TryRelease(item)) //Tf the pool is full
            {
                using (SyncRoot.Enter())
                {
                    if (IsDisposing || IsDisposed)
                    {
                        throw new ObjectDisposedException(nameof(PrimitiveObjectPool<T>));
                    }

                    m_internalStorage.Add(item); //This will increment storage bag count
                    Size++;
                }
            }
        }
        #endregion
    }
}
