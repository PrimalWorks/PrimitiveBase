using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PBase.Collections
{
    public sealed class BoundedBlockingQueue<T> : BaseDisposable, ICollection<T> where T : class
    {
        //Possible additions
        // - Using cancellation tokens
        // - CompleteEnqueuing method
        // - Different timeout parameters for entering thread lock and waiting for free space to enqueue
        //      item to peek or dequeue

        #region Private Fields
        private readonly int m_size;    // The maximum size of the queue
        private T[] m_items;            // The internal array to store queue items
        private int m_count;            // The current number of items in the queue
        private int m_head;             // The head of the queue, used in peeking and dequeuing
        private int m_tail;             // The tail of the queue, used when enqueuing
        #endregion

        #region Public Properties
        public int Count
        {
            get
            {
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                }

                return m_count;
            }
        }

        public int Size
        {
            get
            {
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                }

                return m_size;
            }
        }

        public SafeLock QueueSyncRoot
        {
            get
            {
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                }

                return SyncRoot;
            }
        }

        public T[] Values
        {
            get
            {
                //Provides a snapshot of current queue in queue order
                using (SyncRoot.Enter())
                {
                    T[] values;

                    if (IsDisposing || IsDisposed)
                    {
                        throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                    }

                    values = new T[m_count];
                    int nPos = m_head;

                    for (int i = 0; i < m_count; i++)
                    {
                        values[i] = m_items[nPos++];
                        nPos %= m_size;
                    }

                    return values;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                }

                return false;
            }
        }
        #endregion

        #region Constructor
        public BoundedBlockingQueue(int size)
        {
            if (size < 1)
            {
                throw new ArgumentOutOfRangeException("size", "size must be greater than zero.");
            }

            m_size = size;
            m_items = new T[m_size];

            m_count = 0;
            m_head = 0;
            m_tail = 0;
        }
        #endregion

        #region Methods
        #region Enqueue/Add Methods
        public void Enqueue(T item, int timeoutMilliseconds = -1)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "item cannot be null");
            }

            if (IsDisposing || IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
            }

            using (IWaitable syncLock = SyncRoot.Enter(timeoutMilliseconds) as IWaitable)
            {
                //While the queue is full, wait for an empty space
                //Timeout won't be exact if multiple enqueues are blocked on a full queue
                //So a dequeue could be followed by a different enqueue so this will wait
                //for another loop
                while (m_count == m_size)
                {
                    //Wait for a pulse from a dequeue
                    if (!syncLock.Wait(timeoutMilliseconds))
                    {
                        throw new TimeoutException("Enqueue timed out while waiting for available queue space");
                    }

                    //Double check not disposed since waiting for available space
                    if (IsDisposing || IsDisposed)
                    {
                        throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                    }
                }

                m_items[m_tail++] = item;   // Place the new item immediately after tail, then increment tail pos
                m_tail %= m_size;           // Modulus new tail pos with size so queue items will wrap around
                m_count++;                  // Count will never be more than size after incrementing here

                //Signal that an item has been enqueued to unblock waiting
                //dequeue threads
                syncLock.PulseAll();
            }
        }

        public void Add(T item)
        {
            //Could use this as another enqueue method but consumer of this BBQ
            //should know it is to be used as a queue
            throw new InvalidOperationException("Add method invalid for a queue");
        }

        public bool TryEnqueue(T item, int timeoutMilliseconds = 0)
        {
            if (item == null)
            {
                return false;
            }

            //The method only throws exception on thread lock timeout
            using (IWaitable syncLock = SyncRoot.Enter(timeoutMilliseconds) as IWaitable)
            {
                if (IsDisposing || IsDisposed)
                {
                    return false;
                }

                if (m_count == m_size)
                {
                    return false;
                }

                m_items[m_tail++] = item;   // Place the new item immediately after tail, then increment tail pos
                m_tail %= m_size;           // Modulus new tail pos with size so queue items will wrap around
                m_count++;                  // Count will never be more than size after incrementing here

                //Signal that an item has been enqueued to unblock waiting
                //dequeue threads
                syncLock.PulseAll();

                return true;
            }
        }
        #endregion

        #region Peek Methods
        public T Peek(int timeoutMilliseconds = -1, bool waitForEnqueue = false)
        {
            using (IWaitable syncLock = SyncRoot.Enter(timeoutMilliseconds) as IWaitable)
            {
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                }

                if (m_count == 0)
                {
                    if (waitForEnqueue)
                    {
                        //While queue is empty, wait for an item to enqueue
                        while (m_count == 0)
                        {
                            //Wait for a signal from an enqueue
                            if (!syncLock.Wait(timeoutMilliseconds))
                            {
                                throw new TimeoutException("Dequeue timed out while waiting for queue item to dequeue");
                            }

                            //Double check not disposed since waiting for item to be enqueued
                            if (IsDisposing || IsDisposed)
                            {
                                throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                            }
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Bounded Blocking Queue is empty");
                    }
                }

                T value = m_items[m_head];
                return value;
            }
        }

        public bool TryPeek(out T item, int timeoutMilliseconds = 0)
        {
            //This method only throws exception on thread lock timeout
            using (SyncRoot.Enter(timeoutMilliseconds))
            {
                if (IsDisposing || IsDisposed)
                {
                    item = null;
                    return false;
                }

                if (m_count == 0)
                {
                    item = null;
                    return false;
                }

                item = m_items[m_head];
                return true;
            }
        }
        #endregion

        #region Dequeue/Remove Methods
        public T Dequeue(int timeoutMilliseconds = -1)
        {
            if (IsDisposing || IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
            }

            using (IWaitable syncLock = SyncRoot.Enter(timeoutMilliseconds) as IWaitable)
            {
                //Similar reasoning for here from Enqueue method
                //While queue is empty, wait for an item to enqueue
                while (m_count == 0)
                {
                    //Wait for a signal from an enqueue
                    if (!syncLock.Wait(timeoutMilliseconds))
                    {
                        throw new TimeoutException("Dequeue timed out while waiting for queue item to dequeue");
                    }

                    //Double check not disposed since waiting for item to be enqueued
                    if (IsDisposing || IsDisposed)
                    {
                        throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                    }
                }

                T value = m_items[m_head];  // Get item at head of queue
                m_items[m_head++] = null;   // Clear item at head of queue then increment head pos
                m_head %= m_size;           // Modulus new head pos with size so queue items will wrap around
                m_count--;                  // Count will never be less than zero after decrementing here 

                //Signal that an item has been dequeued to unblock waiting
                //enqueue threads
                syncLock.PulseAll();

                return value;
            }
        }

        public bool TryDequeue(out T item, int timeoutMilliseconds = 0)
        {
            //This method only throws exception on thread lock timeout
            using (IWaitable syncRoot = SyncRoot.Enter(timeoutMilliseconds) as IWaitable)
            {
                if (IsDisposing || IsDisposed)
                {
                    item = null;
                    return false;
                }

                if (m_count == 0)
                {
                    item = null;
                    return false;
                }

                item = m_items[m_head];     // Get item at head of queue
                m_items[m_head++] = null;   // Clear item at head of queue then increment head pos
                m_head %= m_size;           // Modulus new head pos with size so queue items will wrap around
                m_count--;                  // Count will never be less than zero after decrementing here 

                //Signal that an item has been dequeued to unblock waiting
                //enqueue threads
                syncRoot.PulseAll();

                return true;
            }
        }

        public bool Remove(T item)
        {
            //Again, could use this as another dequeue method but consumer of this BBQ
            //should know it is to be used as a queue
            throw new InvalidOperationException("Remove method invalid for a queue");
        }
        #endregion

        #region Other ICollection Methods
        public bool Contains(T item)
        {
            //Use snapshot of current queue values
            return Values.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            //Use snapshot of current queue values
            T[] tmpArray = Values;
            tmpArray.CopyTo(array, arrayIndex);
        }

        public void Clear()
        {
            using (IWaitable syncLock = SyncRoot.Enter() as IWaitable)
            {
                //Unblock waiting threads
                syncLock.PulseAll();

                m_count = 0;
                m_head = 0;
                m_tail = 0;
                m_items = new T[m_size];
            }
        }
        #endregion

        #region Dispose Method
        protected override void Dispose(bool disposing)
        {
            using (SyncRoot.Enter())
            {
                Clear();

                m_items = null;
            }
        }
        #endregion

        #region Enumerator Getters
        public IEnumerator<T> GetEnumerator()
        {
            //Using Values Enumerator
            return (IEnumerator<T>)Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            //Using Values Enumerator
            return Values.GetEnumerator();
        }
        #endregion
        #endregion
    }
}
