using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections;

namespace PBase.Utility
{
    /// <summary>
    /// Provides a thread-safe dictionary for use with data binding.
    /// </summary>
    /// <typeparam name="TKey">Specifies the type of the keys in this collection.</typeparam>
    /// <typeparam name="TValue">Specifies the type of the values in this collection.</typeparam>
    public class ConcurrentObservableDictionary<TKey, TValue> :
        ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>,
        INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>Event raised when the collection changes.</summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>Event raised when a property on the collection changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly SynchronizationContext m_context;
        private readonly ConcurrentDictionary<TKey, TValue> m_dictionary;

        /// <summary>
        /// Initializes an instance of the ObservableConcurrentDictionary class.
        /// </summary>
        public ConcurrentObservableDictionary()
        {
            m_context = AsyncOperationManager.SynchronizationContext;
            m_dictionary = new ConcurrentDictionary<TKey, TValue>();
        }
        
        /// <summary>
        /// Notifies observers of CollectionChanged or PropertyChanged of an update to the dictionary.
        /// </summary>
        private void NotifyObservers(NotifyCollectionChangedAction action)
        {
            var collectionHandler = CollectionChanged;
            var propertyHandler = PropertyChanged;
            if (collectionHandler != null)
            {
                m_context.Post(s =>
                {
                    collectionHandler?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
                }, null);
            }
        }

        /// <summary>Attempts to add an item to the dictionary, notifying observers of any changes.</summary>
        /// <param name="item">The item to be added.</param>
        /// <returns>Whether the add was successful.</returns>
        private bool TryAddWithNotification(KeyValuePair<TKey, TValue> item)
        {
            return TryAddWithNotification(item.Key, item.Value);
        }

        /// <summary>Attempts to add an item to the dictionary, notifying observers of any changes.</summary>
        /// <param name="key">The key of the item to be added.</param>
        /// <param name="value">The value of the item to be added.</param>
        /// <returns>Whether the add was successful.</returns>
        private bool TryAddWithNotification(TKey key, TValue value)
        {
            if (!m_dictionary.TryAdd(key, value))
            {
                return false;
            } 

            NotifyObservers(NotifyCollectionChangedAction.Add);
            return true;
        }

        /// <summary>Attempts to remove an item from the dictionary, notifying observers of any changes.</summary>
        /// <param name="key">The key of the item to be removed.</param>
        /// <param name="value">The value of the item removed.</param>
        /// <returns>Whether the removal was successful.</returns>
        private bool TryRemoveWithNotification(TKey key, out TValue value)
        {
            if (!m_dictionary.TryRemove(key, out value))
            {
                return false;
            } 

            NotifyObservers(NotifyCollectionChangedAction.Remove);
            return true;
        }

        /// <summary>Attempts to add or update an item in the dictionary, notifying observers of any changes.</summary>
        /// <param name="key">The key of the item to be updated.</param>
        /// <param name="value">The new value to set for the item.</param>
        /// <returns>Whether the update was successful.</returns>
        private void UpdateWithNotification(TKey key, TValue value)
        {
            m_dictionary[key] = value;
            NotifyObservers(NotifyCollectionChangedAction.Replace);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            TryAddWithNotification(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            if (m_dictionary.IsEmpty)
            {
                return;
            }

            m_dictionary.Clear();
            NotifyObservers(NotifyCollectionChangedAction.Reset);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)m_dictionary).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)m_dictionary).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return TryRemoveWithNotification(item.Key, out _);
        }

        int ICollection<KeyValuePair<TKey, TValue>>.Count => ((ICollection<KeyValuePair<TKey, TValue>>)m_dictionary).Count;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)m_dictionary).IsReadOnly;

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)m_dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)m_dictionary).GetEnumerator();
        }

        public void Add(TKey key, TValue value)
        {
            TryAddWithNotification(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return m_dictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys => m_dictionary.Keys;

        public bool Remove(TKey key)
        {
            return TryRemoveWithNotification(key, out _);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_dictionary.TryGetValue(key, out value);
        }

        public bool TryAdd(TKey key, TValue value)
        {

        }

        public bool TryRemove(TKey key, TValue value)
        {

        }

        public bool TryReplace(TKey key, TValue value)
        {

        }

        public bool AddOrUpdate(TKey key, TValue value)
        {

        }

        public bool GetOrAdd(TKey key, TValue value)
        {

        }

        public ICollection<TValue> Values => m_dictionary.Values;

        public TValue this[TKey key]
        {
            get => m_dictionary[key];
            set => UpdateWithNotification(key, value);
        }
    }
}