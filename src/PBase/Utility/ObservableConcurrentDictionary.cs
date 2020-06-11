using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace PBase.Utility
{
    /// <summary>
    ///     Provides a thread-safe dictionary for use with data binding.
    /// </summary>
    /// <typeparam name="TKey">Specifies the type of the keys in this collection.</typeparam>
    /// <typeparam name="TValue">Specifies the type of the values in this collection.</typeparam>
    public class ObservableConcurrentDictionary<TKey, TValue> :
        ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>,
        INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly SynchronizationContext m_context;
        private readonly ConcurrentDictionary<TKey, TValue> m_dictionary;

        /// <summary>
        ///     Initializes an instance of the ObservableConcurrentDictionary class.
        /// </summary>
        public ObservableConcurrentDictionary()
        {
            m_context = AsyncOperationManager.SynchronizationContext;
            m_dictionary = new ConcurrentDictionary<TKey, TValue>();
        }

        /// <summary>
        ///     Initializes an instance of the ObservableConcurrentDictionary class with a collection changed and property changed
        ///     event handler.
        /// </summary>
        /// <param name="collectionChanged">The collection changed event handler.</param>
        /// <param name="propertyChanged">The property changed event handler.</param>
        public ObservableConcurrentDictionary(NotifyCollectionChangedEventHandler collectionChanged,
            PropertyChangedEventHandler propertyChanged) : this()
        {
            CollectionChanged += collectionChanged;
            PropertyChanged += propertyChanged;
        }

        /// <summary>
        ///     Adds the specified key/value pair to the dictionary.
        /// </summary>
        /// <param name="item">The key/value pair to add.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if key is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if an element with the same key already exists in the dictionary.</exception>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        ///     Removes the first instance of the specified key/value pair from the dictionary.
        /// </summary>
        /// <param name="item">The key/value pair to remove.</param>
        /// <returns>
        ///     True if item was successfully removed from the dictionary; otherwise, false. This method also returns false if
        ///     item is not found in the original dictionary.
        /// </returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!((IDictionary<TKey, TValue>) m_dictionary).Remove(item)) return false;

            var propertiesChanged = new List<string> {"Count", "Keys", "Values"};

            NotifyObservers(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item),
                propertiesChanged);

            return true;
        }

        /// <summary>
        ///     Gets the number of key/value pairs contained in the dictionary.
        /// </summary>
        public int Count => m_dictionary.Count;

        /// <summary>
        ///     Returns if the dictionary is read only; it is not.
        /// </summary>
        public bool IsReadOnly => false;


        /// <summary>
        ///     Removes all keys and values from the dictionary.
        /// </summary>
        public void Clear()
        {
            m_dictionary.Clear();

            var propertiesChanged = new List<string> {"Count", "Keys", "Values"};

            NotifyObservers(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset),
                propertiesChanged);
        }

        /// <summary>
        ///     Determines wether the dictionary contains the specified key/value pair.
        /// </summary>
        /// <param name="item">The key/value pair to locate.</param>
        /// <returns>
        ///     True if item is found in the dictionary; otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>) m_dictionary).Contains(item);
        }

        /// <summary>
        ///     Copies the elements of the dictionary to an Array of key/value pairs, starting at a particular Array index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional Array that is the destination of the elements copied from the dictionary. The
        ///     Array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <returns>
        ///     True if item is found in the dictionary; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if array is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if arrayIndex is less than 0.</exception>
        /// <exception cref="System.ArgumentException">
        ///     Thrown if the number of elements in the dictionary is greater than the
        ///     available space from arrayIndex to the end of the destination array.
        /// </exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>) this).CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the dictionary.
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>) m_dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if key is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if an element with the same key already exists in the dictionary.</exception>
        public void Add(TKey key, TValue value)
        {
            ((IDictionary<TKey, TValue>) m_dictionary).Add(key, value);

            var newItem = new KeyValuePair<TKey, TValue>(key, value);
            var propertiesChanged = new List<string> {"Count", "Keys", "Values"};

            NotifyObservers(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem),
                propertiesChanged);
        }

        /// <summary>
        ///     Removes the value with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        ///     True if the element is successfully found and removed; otherwise, false. This method returns false if key is
        ///     not found in the dictionary.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if key is null.</exception>
        public bool Remove(TKey key)
        {
            return TryRemove(key, out _);
        }

        /// <summary>
        ///     Attempts to get and return the value with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the object from the dictionary that has the specified key, or
        ///     the default value of the type if the operation failed.
        /// </param>
        /// <returns>
        ///     True if the key was found in the dictionary; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if key is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get => m_dictionary[key];
            set => TryUpdate(key, value, m_dictionary[key]);
        }

        /// <summary>
        ///     Gets a collection containing the keys in the dictionary.
        /// </summary>
        public ICollection<TKey> Keys => m_dictionary.Keys;

        /// <summary>
        ///     Gets a collection containing the values in the dictionary.
        /// </summary>
        public ICollection<TValue> Values => m_dictionary.Values;

        /// <summary>
        ///     Determines whether the dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns>
        ///     True if the dictionary contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if key is null.</exception>
        public bool ContainsKey(TKey key)
        {
            return m_dictionary.ContainsKey(key);
        }

        /// <summary>Event raised when the collection changes.</summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>Event raised when a property on the collection changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Notifies observers by invoking the collection changed and property changed event handlers.
        /// </summary>
        private void NotifyObservers(NotifyCollectionChangedEventArgs action, IEnumerable<string> propertiesChanged)
        {
            m_context.Post(s =>
            {
                OnCollectionChanged(action);
                OnPropertyChanged(propertiesChanged);
            }, null);
        }

        /// <summary>
        ///     Invokes the collection changed event handler if one exists.
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs action)
        {
            var collectionHandler = CollectionChanged;
            collectionHandler?.Invoke(this, action);
        }

        /// <summary>
        ///     Invokes the propety changed event handler if one exists; once for each property changed.
        /// </summary>
        private void OnPropertyChanged(IEnumerable<string> propertiesChanged)
        {
            var propertyHandler = PropertyChanged;
            foreach (var property in propertiesChanged)
                propertyHandler?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        ///     Attempts to add the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        /// <returns>
        ///     True if the key/value pair was added to the dictionary successfully; false if the key already exists.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if key is null.</exception>
        /// <exception cref="System.OverflowException">Thrown if the dictionary contains too many elements.</exception>
        public bool TryAdd(TKey key, TValue value)
        {
            if (!m_dictionary.TryAdd(key, value)) return false;

            var newItem = new KeyValuePair<TKey, TValue>(key, value);
            var propertiesChanged = new List<string> {"Count", "Keys", "Values"};

            NotifyObservers(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem),
                propertiesChanged);

            return true;
        }

        /// <summary>
        ///     Attempts to remove and return the value that has the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">
        ///     When this method returns, contains the object removed from the dictionary, or the default value of
        ///     the TValue type if key does not exist.
        /// </param>
        /// <returns>
        ///     True if the object was removed successfully; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if key is null.</exception>
        public bool TryRemove(TKey key, out TValue value)
        {
            if (!m_dictionary.TryRemove(key, out value)) return false;

            var oldItem = new KeyValuePair<TKey, TValue>(key, value);
            var propertiesChanged = new List<string> {"Count", "Keys", "Values"};

            NotifyObservers(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem),
                propertiesChanged);

            return true;
        }

        /// <summary>
        ///     Updates the value associated with key to newValue if the existing value with key is equal to comparisonValue.
        /// </summary>
        /// <param name="key">The key of the value that is compared with comparisonValue and possibly replaced.</param>
        /// <param name="newValue">
        ///     The value that replaces the value of the element that has the specified key if the comparison
        ///     results in equality.
        /// </param>
        /// <param name="comparisonValue">The value that is compared with the value of the element that has the specified key.</param>
        /// <returns>
        ///     True if the value with key was equal to comparisonValue and was replaced with newValue; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if key is null.</exception>
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            if (!m_dictionary.TryUpdate(key, newValue, comparisonValue)) return false;

            var newItem = new KeyValuePair<TKey, TValue>(key, newValue);
            var oldItem = new KeyValuePair<TKey, TValue>(key, comparisonValue);
            var propertiesChanged = new List<string> {"Keys", "Values"};

            NotifyObservers(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem),
                propertiesChanged);

            return true;
        }

        /// <summary>
        ///     Returns the value of a key/value pair if the key exists, otherwise adds a new key/value pair to the dictionary and
        ///     returns its value.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value to be added if the key does not already exist.</param>
        /// <returns>
        ///     The value for the key. Either the existing value if the key is already in the dictionary, or the new value if
        ///     it was not.
        /// </returns>
        public TValue GetOrAdd(TKey key, TValue value)
        {
            if (TryGetValue(key, out var getValue)) return getValue;

            Add(key, value);
            return value;
        }

        /// <summary>
        ///     Adds a key/value pair to the dictionary if the key does not already exist, or updates a key/value pair in the
        ///     dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key of the element to add or update.</param>
        /// <param name="value">The value to be added if the key does not already exist; or updated with if it does.</param>
        /// <returns>
        ///     True if the value was added or updated successfully; otherwise, false.
        /// </returns>
        public bool AddOrUpdate(TKey key, TValue value)
        {
            if (TryAdd(key, value)) return true;

            var comparisonValue = m_dictionary[key];
            return TryUpdate(key, value, comparisonValue);
        }
    }
}