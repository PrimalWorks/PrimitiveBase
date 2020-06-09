using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PBase.Collections
{
    public sealed class BoundedBlockingQueue<T> : BaseDisposable, ICollection<T> where T : class
    {
        #region Private Fields
        private readonly int m_size;
        private T[] m_items;
        private int m_count;
        private int m_head;
        private int m_tail;
        private AutoResetEvent m_resetEvent;
        #endregion

        //TODO
        //source code documentation w/ author
        //
        //could possibly add option for different timeouts
        //so someone might want to wait indefinitely for a lock
        //but not for an available space in the queue
        //could add task cancellation tokens for overloads
        //complete adding method? to disallow future additions

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
            m_resetEvent = new AutoResetEvent(false);
        }
        #endregion

        #region Methods
        #region Add/Enqueue Methods
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

            //This needs to be outside the safe lock as an enqueue thread
            //will get stuck waiting for a dequeue which is waiting for this enqueue
            //SafeLock doesn't have a Wait method
            if (m_count == m_size)
            {
                //Theoretically possible for an enqueue to set immediately
                //after resetting here, meaning below would fall through to
                //enqueueing even if the queue is full which will
                //overwrite the head
                //Could change to while loop though timeout may not be
                //adhered to as strictly as expected though that might not
                //be a priority - though even then still possible for
                //an item to be enqueued before exiting the while loop

                //Clear event signal to wait for a signal from a dequeue
                //Could use separate dequeue and enqueue reset events
                m_resetEvent.Reset();
                
                //Wait for a set from a dequeue
                if (!m_resetEvent.WaitOne(timeoutMilliseconds))
                {
                    throw new TimeoutException("Enqueue timed out while waiting for available queue space");
                }
            }

            using (SyncRoot.Enter(timeoutMilliseconds))
            {
                //Double check not disposed since waiting for available space
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                }

                m_items[m_tail++] = item;
                m_tail %= m_size;
                m_count++;

                //Signal that an item has been enqueued to unblock waiting
                //dequeue threads
                m_resetEvent.Set();

                return true;
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
            //The method only throws exception on thread lock timeout
            using (SyncRoot.Enter(timeoutMilliseconds))
            {
                if (item == null)
                {
                    return false;
                }

                if (IsDisposing || IsDisposed)
                {
                    return false;
                }

                if (m_count == m_size)
                {
                    return false;
                }

                m_items[m_tail++] = item;
                m_tail %= m_size;
                m_count++;

                //Signal that an item has been enqueued to unblock waiting
                //dequeue threads
                m_resetEvent.Set();

                return true;
            }
        }
        #endregion

        #region Peek Methods
        public T Peek(int timeoutMilliseconds = -1)
        {
            using (SyncRoot.Enter(timeoutMilliseconds))
            {
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                }

                //Could wait for an enqueue here
                if (m_count == 0)
                {
                    throw new InvalidOperationException("Bounded Blocking Queue is empty");
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

        #region Remove/Dequeue Methods
        public T Dequeue(int timeoutMilliseconds = -1)
        {
            if (IsDisposing || IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
            }

            //Similar reasoning for here from Enqueue method
            if (m_count == 0)
            {
                //Clear event signal to wait for a signal from a dequeue
                //Could use separate dequeue and enqueue reset events
                m_resetEvent.Reset();

                //Wait for a set from an enqueue
                if (!m_resetEvent.WaitOne(timeoutMilliseconds))
                {
                    throw new TimeoutException("Dequeue timed out while waiting for queue item to dequeue");
                }
            }

            using (SyncRoot.Enter(timeoutMilliseconds))
            {
                //Double check not disposed since waiting for item to be enqueued
                if (IsDisposing || IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(BoundedBlockingQueue<T>));
                }

                T value = m_items[m_head];
                m_items[m_head++] = null;
                m_head %= m_size;
                m_count--;

                //Signal that an item has been dequeued to unblock waiting
                //enqueue threads
                m_resetEvent.Set();

                return value;
            }
        }

        public bool TryDequeue(out T item, int timeoutMilliseconds = 0)
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
                m_items[m_head++] = null;
                m_head %= m_size;
                m_count--;

                //Signal that an item has been dequeued to unblock waiting
                //enqueue threads
                m_resetEvent.Set();

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
            using (SyncRoot.Enter())
            {
                //Unblock waiting threads
                PulseWaitingThreads();

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

                m_resetEvent.Close();

                m_items = null;
            }
        }
        #endregion

        #region Misc Methods

        private void PulseWaitingThreads()
        {
            //This only needs to be called once as each successive enqueue
            //will set the event for the next waiting thread
            //though these enqueues will be invalid
            //hence why this should only be called when clearing or disposing
            m_resetEvent.Set();
        }

        #endregion

        #region Enumerator Getters
        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }
        #endregion
        #endregion
    }
}
