using System;
using System.Collections.Concurrent;

namespace PBase.Collections
{
    public class ResizablePrimitiveObjectPool<T> : PrimitiveObjectPool<T> where T : class, new()
    {
        #region Constructor
        public ResizablePrimitiveObjectPool(int size) : base(size) {}
        #endregion

        #region Overridden Obtain and Release methods
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
