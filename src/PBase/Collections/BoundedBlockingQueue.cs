using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PBase.Collections
{
    public sealed class BoundedBlockingQueue<T> : BaseDisposable, ICollection<T> where T : class
    {
        private readonly int m_nSize;            // Max number of elements queue can hold without blocking.
        private T[] m_aItems;           // Buffer used to store queue objects with max "Size".
        private int m_nCount;           // Current number of elements in the queue.
        private int m_nHead;            // Index of slot for object to remove on next Dequeue. 
        private int m_nTail;            // Index of slot for next Enqueue object.
        private readonly AutoResetEvent m_evQueue;

        //TODO
        //clear
        //dispose
        //unit tests
        //source code documentation
        //comments
        //get enumerator methods are a question mark
        //regions

        public int Count
        {
            get => m_nCount;
        }

        public int Size
        {
            get => m_nSize;
        }

        public SafeLock QueueSyncRoot
        {
            get => SyncRoot;
        }

        public T[] Values
        {
            get
            {
                using (SyncRoot.Enter())
                {
                    T[] values;

                    if (IsDisposing || IsDisposed)
                    {
                        throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                    }

                    values = new T[m_nCount];
                    int nPos = m_nHead;

                    for (int i = 0; i < m_nCount; i++)
                    {
                        values[i] = m_aItems[nPos++];
                        nPos %= m_nSize;
                    }

                    return values;
                }
            }
        }

        public bool IsReadOnly => false;

        public BoundedBlockingQueue(int size)
        {
            if (size < 1)
            {
                throw new ArgumentOutOfRangeException("size", "size must be greater than zero.");
            }

            m_nSize = size;
            m_aItems = new T[m_nSize];

            m_nCount = 0;
            m_nHead = 0;
            m_nTail = 0;
            m_evQueue = new AutoResetEvent(false);
        }

        //blocking 
        //if timeout > timeout.inf
        //will timeout either waiting for sync root or waiting for space in queue to
        //become available
        //will throw if timeout on waiting for sync root
        public bool Enqueue(T item, int timeoutMilliseconds = -1)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "item cannot be null");
            }

            if (IsDisposing || IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
            }

            while (m_nCount == m_nSize)
            {
                if (!m_evQueue.WaitOne(timeoutMilliseconds))
                {
                    throw new TimeoutException("Enqueue timed out while waiting for available queue space");
                }

                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                }
            }

            using (SyncRoot.Enter(timeoutMilliseconds))
            {
                m_aItems[m_nTail++] = item;
                m_nTail %= m_nSize;
                m_nCount++;

                m_evQueue.Set();

                return true;
            }
        }

        public void Add(T item)
        {
            Enqueue(item);
        }

        public bool TryEnqueue(T item)
        {
            using (SyncRoot.Enter(timeoutMilliseconds: 0))
            {
                if (item == null)
                {
                    return false;
                }

                if (IsDisposing || IsDisposed)
                {
                    return false;
                }

                if (m_nCount == m_nSize)
                {
                    return false;
                }

                m_aItems[m_nTail++] = item;
                m_nTail %= m_nSize;
                m_nCount++;

                m_evQueue.Set();

                return true;
            }
        }

        public T Peek()
        {
            using (SyncRoot.Enter())
            {
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                }

                if (m_nCount == 0)
                {
                    throw new InvalidOperationException("Bounded Blocking Queue is empty");
                }

                T value = m_aItems[m_nHead];
                return value;
            }
        }

        public bool TryPeek(out T item)
        {
            using (SyncRoot.Enter())
            {
                if (IsDisposing || IsDisposed)
                {
                    item = null;
                    return false;
                }

                if (m_nCount == 0)
                {
                    item = null;
                    return false;
                }

                item = m_aItems[m_nHead];
                return true;
            }
        }

        public T Dequeue(int timeoutMilliseconds = -1)
        {
            if (IsDisposing || IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
            }

            while (m_nCount == 0)
            {
                if (!m_evQueue.WaitOne(timeoutMilliseconds))
                {
                    throw new TimeoutException("Dequeue timed out while waiting for queue item to dequeue");
                }

                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                }
            }

            using (SyncRoot.Enter(timeoutMilliseconds))
            {
                T value = m_aItems[m_nHead];
                m_aItems[m_nHead++] = null;
                m_nHead %= m_nSize;
                m_nCount--;

                m_evQueue.Set();

                return value;
            }
        }

        public bool TryDequeue(out T item)
        {
            using (SyncRoot.Enter(timeoutMilliseconds: 0))
            {
                if (IsDisposing || IsDisposed)
                {
                    item = null;
                    return false;
                }

                if (m_nCount == 0)
                {
                    item = null;
                    return false;
                }

                item = m_aItems[m_nHead];
                m_aItems[m_nHead++] = null;
                m_nHead %= m_nSize;
                m_nCount--;

                m_evQueue.Set();

                return true;
            }
        }

        //will only throw if item is not the tail
        // otherise, just return dequeue
        public bool Remove(T item)
        {
            using (SyncRoot.Enter())
            {
                if (m_aItems[m_nHead] == item)
                {
                    return TryDequeue(out item);
                }
                else
                {
                    return false;
                }
            }
        }

        public void Clear()
        {
            //using (SyncRoot.Enter())
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            return Values.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            T[] tmpArray = Values;
            tmpArray.CopyTo(array, arrayIndex);
        }

        protected override void Dispose(bool disposing)
        {
            //using (SyncRoot.Enter())
            //close evt handle
            throw new NotImplementedException();
        }

        //we could have enumerator on the values array?
        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)Values.GetEnumerator();
            //throw new NotImplementedException("No Enumerator on Bounded Blocking Queue");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
            //throw new NotImplementedException("No Enumerator on Bounded Blocking Queue");
        }
    }
}
