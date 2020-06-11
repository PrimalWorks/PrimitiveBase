using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using PBase.Utility;
using Xunit;

namespace PBase.Test.Utility
{
    public class ObservableConcurrentDictionaryTests
    {
        private static ObservableConcurrentDictionary<string, string> PerformObservableDictionarySetup(
            ICollection<KeyValuePair<string, string>> items)
        {
            var setupLatch = new CountdownEvent(4 * items.Count);

            void SetupCollectionHandler(object sender, NotifyCollectionChangedEventArgs args)
            {
                setupLatch.Signal();
            }

            void SetupPropertyHandler(object sender, PropertyChangedEventArgs args)
            {
                setupLatch.Signal();
            }

            var observableDictionary =
                new ObservableConcurrentDictionary<string, string>(SetupCollectionHandler,
                    SetupPropertyHandler);

            foreach (var item in items) observableDictionary.Add(item);

            setupLatch.Wait();

            observableDictionary.CollectionChanged -= SetupCollectionHandler;
            observableDictionary.PropertyChanged -= SetupPropertyHandler;

            return observableDictionary;
        }

        [Fact]
        public void TestAdd()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>>());

            var latch = new CountdownEvent(4);

            var collectionChangedActions = new List<NotifyCollectionChangedAction>();
            var propertiesChanged = new List<string>();

            void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
            {
                collectionChangedActions.Add(args.Action);
                latch?.Signal();
            }

            void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
            {
                propertiesChanged.Add(args.PropertyName);
                latch?.Signal();
            }
            
            observableDictionary.CollectionChanged += CollectionChangedHandler;
            observableDictionary.PropertyChanged += PropertyChangedHandler;

            observableDictionary.Add(item.Key, item.Value);

            latch.Wait();

            Assert.Single(observableDictionary);

            var keys = new string[1];
            observableDictionary.Keys.CopyTo(keys, 0);
            Assert.Equal("1", keys[0]);

            var values = new string[1];
            observableDictionary.Values.CopyTo(values, 0);
            Assert.Equal("value", values[0]);

            Assert.Single(collectionChangedActions);
            Assert.Equal(NotifyCollectionChangedAction.Add, collectionChangedActions[0]);

            Assert.Equal(3, propertiesChanged.Count);
            Assert.Contains("Count", propertiesChanged);
            Assert.Contains("Keys", propertiesChanged);
            Assert.Contains("Values", propertiesChanged);

            latch.Dispose();
        }

        [Fact]
        public void TestAddOrUpdateToAdd()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var newItem = new KeyValuePair<string, string>("2", "newValue");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var latch = new CountdownEvent(4);

            var collectionChangedActions = new List<NotifyCollectionChangedAction>();
            var propertiesChanged = new List<string>();

            void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
            {
                collectionChangedActions.Add(args.Action);
                latch?.Signal();
            }

            void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
            {
                propertiesChanged.Add(args.PropertyName);
                latch?.Signal();
            }

            observableDictionary.CollectionChanged += CollectionChangedHandler;
            observableDictionary.PropertyChanged += PropertyChangedHandler;

            observableDictionary.AddOrUpdate(newItem.Key, newItem.Value);

            latch.Wait();

            Assert.Equal(2, observableDictionary.Count);
            Assert.Equal(newItem.Value, observableDictionary[newItem.Key]);

            Assert.Single(collectionChangedActions);
            Assert.Equal(NotifyCollectionChangedAction.Add, collectionChangedActions[0]);

            Assert.Equal(3, propertiesChanged.Count);
            Assert.Contains("Count", propertiesChanged);
            Assert.Contains("Keys", propertiesChanged);
            Assert.Contains("Values", propertiesChanged);
        }

        [Fact]
        public void TestAddOrUpdateToUpdate()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var newItem = new KeyValuePair<string, string>("1", "newValue");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var latch = new CountdownEvent(3);

            var collectionChangedActions = new List<NotifyCollectionChangedAction>();
            var propertiesChanged = new List<string>();

            void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
            {
                collectionChangedActions.Add(args.Action);
                latch?.Signal();
            }

            void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
            {
                propertiesChanged.Add(args.PropertyName);
                latch?.Signal();
            }

            observableDictionary.CollectionChanged += CollectionChangedHandler;
            observableDictionary.PropertyChanged += PropertyChangedHandler;

            observableDictionary.AddOrUpdate(newItem.Key, newItem.Value);

            latch.Wait();

            Assert.Single(observableDictionary);
            Assert.Equal(newItem.Value, observableDictionary[newItem.Key]);

            Assert.Single(collectionChangedActions);
            Assert.Equal(NotifyCollectionChangedAction.Replace, collectionChangedActions[0]);

            Assert.Equal(2, propertiesChanged.Count);
            Assert.Contains("Keys", propertiesChanged);
            Assert.Contains("Values", propertiesChanged);
        }

        [Fact]
        public void TestAddOrUpdateWithNullKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});
            Assert.Throws<ArgumentNullException>(() => { observableDictionary.AddOrUpdate(null, "newValue"); });
        }

        [Fact]
        public void TestAddWithKeyValuePair()
        {
            var latch = new CountdownEvent(4);

            var collectionChangedActions = new List<NotifyCollectionChangedAction>();
            var propertiesChanged = new List<string>();

            void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
            {
                collectionChangedActions.Add(args.Action);
                latch?.Signal();
            }

            void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
            {
                propertiesChanged.Add(args.PropertyName);
                latch?.Signal();
            }

            var observableDictionary =
                new ObservableConcurrentDictionary<string, string>(CollectionChangedHandler, PropertyChangedHandler);

            var item = new KeyValuePair<string, string>("1", "value");
            observableDictionary.Add(item);

            latch.Wait();

            Assert.Single(observableDictionary);

            var keys = new string[1];
            observableDictionary.Keys.CopyTo(keys, 0);
            Assert.Equal("1", keys[0]);

            var values = new string[1];
            observableDictionary.Values.CopyTo(values, 0);
            Assert.Equal("value", values[0]);

            Assert.Single(collectionChangedActions);
            Assert.Equal(NotifyCollectionChangedAction.Add, collectionChangedActions[0]);

            Assert.Equal(3, propertiesChanged.Count);
            Assert.Contains("Count", propertiesChanged);
            Assert.Contains("Keys", propertiesChanged);
            Assert.Contains("Values", propertiesChanged);

            latch.Dispose();
        }

        [Fact]
        public void TestAddWithNullKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});
            Assert.Throws<ArgumentNullException>(() => observableDictionary.Add(null, "value"));
        }

        [Fact]
        public void TestGet()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var result = observableDictionary[item.Key];

            Assert.Equal(item.Value, result);
        }

        [Fact]
        public void TestGetOrAddToAdd()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var newItem = new KeyValuePair<string, string>("2", "newValue");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var latch = new CountdownEvent(4);

            var collectionChangedActions = new List<NotifyCollectionChangedAction>();
            var propertiesChanged = new List<string>();

            void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
            {
                collectionChangedActions.Add(args.Action);
                latch?.Signal();
            }

            void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
            {
                propertiesChanged.Add(args.PropertyName);
                latch?.Signal();
            }

            observableDictionary.CollectionChanged += CollectionChangedHandler;
            observableDictionary.PropertyChanged += PropertyChangedHandler;

            var value = observableDictionary.GetOrAdd(newItem.Key, newItem.Value);

            latch.Wait();

            Assert.Equal(2, observableDictionary.Count);
            Assert.Equal(newItem.Value, observableDictionary[newItem.Key]);

            Assert.Single(collectionChangedActions);
            Assert.Equal(NotifyCollectionChangedAction.Add, collectionChangedActions[0]);

            Assert.Equal(3, propertiesChanged.Count);
            Assert.Contains("Count", propertiesChanged);
            Assert.Contains("Keys", propertiesChanged);
            Assert.Contains("Values", propertiesChanged);
        }

        [Fact]
        public void TestGetOrAddWithNullKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});
            Assert.Throws<ArgumentNullException>(() => observableDictionary.GetOrAdd(null, "newValue"));
        }

        [Fact]
        public void TestGetWithNullKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});
            Assert.Throws<ArgumentNullException>(() =>
            {
                var result = observableDictionary[null];
            });
        }

        [Fact]
        public void TestRemove()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var latch = new CountdownEvent(4);

            var collectionChangedActions = new List<NotifyCollectionChangedAction>();
            var propertiesChanged = new List<string>();

            void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
            {
                collectionChangedActions.Add(args.Action);
                latch?.Signal();
            }

            void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
            {
                propertiesChanged.Add(args.PropertyName);
                latch?.Signal();
            }

            observableDictionary.CollectionChanged += CollectionChangedHandler;
            observableDictionary.PropertyChanged += PropertyChangedHandler;

            var success = observableDictionary.Remove(item.Key);

            latch.Wait();

            Assert.True(success);

            Assert.Empty(observableDictionary);

            Assert.Single(collectionChangedActions);
            Assert.Equal(NotifyCollectionChangedAction.Remove, collectionChangedActions[0]);

            Assert.Equal(3, propertiesChanged.Count);
            Assert.Contains("Count", propertiesChanged);
            Assert.Contains("Keys", propertiesChanged);
            Assert.Contains("Values", propertiesChanged);
        }

        [Fact]
        public void TestRemoveWithInvalidKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");

            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var success = observableDictionary.Remove("invalid");

            Assert.False(success);

            Assert.Single(observableDictionary);
        }

        [Fact]
        public void TestRemoveWithNullKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});
            Assert.Throws<ArgumentNullException>(() => observableDictionary.Remove(null));
        }

        [Fact]
        public void TestTryAdd()
        {
            var latch = new CountdownEvent(4);

            var collectionChangedActions = new List<NotifyCollectionChangedAction>();
            var propertiesChanged = new List<string>();

            void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
            {
                collectionChangedActions.Add(args.Action);
                latch?.Signal();
            }

            void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
            {
                propertiesChanged.Add(args.PropertyName);
                latch?.Signal();
            }

            var observableDictionary =
                new ObservableConcurrentDictionary<string, string>(CollectionChangedHandler, PropertyChangedHandler);

            var success = observableDictionary.TryAdd("1", "value");

            latch.Wait();

            Assert.True(success);

            Assert.Single(observableDictionary);

            var keys = new string[1];
            observableDictionary.Keys.CopyTo(keys, 0);
            Assert.Equal("1", keys[0]);

            var values = new string[1];
            observableDictionary.Values.CopyTo(values, 0);
            Assert.Equal("value", values[0]);

            Assert.Single(collectionChangedActions);
            Assert.Equal(NotifyCollectionChangedAction.Add, collectionChangedActions[0]);

            Assert.Equal(3, propertiesChanged.Count);
            Assert.Contains("Count", propertiesChanged);
            Assert.Contains("Keys", propertiesChanged);
            Assert.Contains("Values", propertiesChanged);

            latch.Dispose();
        }

        [Fact]
        public void TestTryAddWithNullKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});
            Assert.Throws<ArgumentNullException>(() => observableDictionary.TryAdd(null, "value"));
        }

        [Fact]
        public void TestTryGetOrAddToGet()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var value = observableDictionary.GetOrAdd(item.Key, "ToAdd");

            Assert.Equal(item.Value, value);
        }

        [Fact]
        public void TestTryGetValue()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var success = observableDictionary.TryGetValue(item.Key, out var result);

            Assert.True(success);
            Assert.Equal(item.Value, result);
        }

        [Fact]
        public void TestTryGetValueWithInvalidKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var success = observableDictionary.TryGetValue("2", out var result);

            Assert.False(success);
            Assert.Equal(default, result);
        }

        [Fact]
        public void TestTryGetValueWithNullKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});
            Assert.Throws<ArgumentNullException>(() => { observableDictionary.TryGetValue(null, out _); });
        }

        [Fact]
        public void TestTryRemove()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var latch = new CountdownEvent(4);

            var collectionChangedActions = new List<NotifyCollectionChangedAction>();
            var propertiesChanged = new List<string>();

            void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
            {
                collectionChangedActions.Add(args.Action);
                latch?.Signal();
            }

            void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
            {
                propertiesChanged.Add(args.PropertyName);
                latch?.Signal();
            }

            observableDictionary.CollectionChanged += CollectionChangedHandler;
            observableDictionary.PropertyChanged += PropertyChangedHandler;

            var success = observableDictionary.TryRemove(item.Key, out var outValue);

            latch.Wait();

            Assert.True(success);

            Assert.Equal(item.Value, outValue);

            Assert.Empty(observableDictionary);

            Assert.Single(collectionChangedActions);
            Assert.Equal(NotifyCollectionChangedAction.Remove, collectionChangedActions[0]);

            Assert.Equal(3, propertiesChanged.Count);
            Assert.Contains("Count", propertiesChanged);
            Assert.Contains("Keys", propertiesChanged);
            Assert.Contains("Values", propertiesChanged);
        }

        [Fact]
        public void TestTryRemoveWithInvalidKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var success = observableDictionary.TryRemove("invalid", out var outValue);

            Assert.False(success);
            Assert.Equal(default, outValue);
            Assert.Single(observableDictionary);
        }

        [Fact]
        public void TestTryRemoveWithNullKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});
            Assert.Throws<ArgumentNullException>(() => observableDictionary.TryRemove(null, out _));
        }

        [Fact]
        public void TestTryUpdate()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var latch = new CountdownEvent(3);

            var collectionChangedActions = new List<NotifyCollectionChangedAction>();
            var propertiesChanged = new List<string>();

            void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
            {
                collectionChangedActions.Add(args.Action);
                latch?.Signal();
            }

            void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
            {
                propertiesChanged.Add(args.PropertyName);
                latch?.Signal();
            }

            observableDictionary.CollectionChanged += CollectionChangedHandler;
            observableDictionary.PropertyChanged += PropertyChangedHandler;

            var success = observableDictionary.TryUpdate(item.Key, "NewValue", item.Value);

            latch.Wait();

            Assert.True(success);

            Assert.Single(observableDictionary);
            Assert.Equal("NewValue", observableDictionary[item.Key]);

            Assert.Single(collectionChangedActions);
            Assert.Equal(NotifyCollectionChangedAction.Replace, collectionChangedActions[0]);

            Assert.Equal(2, propertiesChanged.Count);
            Assert.Contains("Keys", propertiesChanged);
            Assert.Contains("Values", propertiesChanged);
        }

        [Fact]
        public void TestTryUpdateWithInvalidKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var success = observableDictionary.TryUpdate("2", "NewValue", item.Value);

            Assert.False(success);

            Assert.Single(observableDictionary);
            Assert.Equal(item.Value, observableDictionary[item.Key]);
        }

        [Fact]
        public void TestTryUpdateWithNullKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});
            Assert.Throws<ArgumentNullException>(() => observableDictionary.TryUpdate(null, "newValue", item.Value));
        }

        [Fact]
        public void TestUpdate()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var latch = new CountdownEvent(3);

            var collectionChangedActions = new List<NotifyCollectionChangedAction>();
            var propertiesChanged = new List<string>();

            void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
            {
                collectionChangedActions.Add(args.Action);
                latch?.Signal();
            }

            void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
            {
                propertiesChanged.Add(args.PropertyName);
                latch?.Signal();
            }

            observableDictionary.CollectionChanged += CollectionChangedHandler;
            observableDictionary.PropertyChanged += PropertyChangedHandler;

            observableDictionary[item.Key] = "NewValue";

            latch.Wait();

            Assert.Single(observableDictionary);
            Assert.Equal("NewValue", observableDictionary[item.Key]);

            Assert.Single(collectionChangedActions);
            Assert.Equal(NotifyCollectionChangedAction.Replace, collectionChangedActions[0]);

            Assert.Equal(2, propertiesChanged.Count);
            Assert.Contains("Keys", propertiesChanged);
            Assert.Contains("Values", propertiesChanged);
        }

        [Fact]
        public void TestUpdateWithNullKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});
            Assert.Throws<ArgumentNullException>(() => observableDictionary[null] = "newValue");
        }

        [Fact]
        public void TestContains()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var contains = observableDictionary.Contains(item);
            Assert.True(contains);

            contains = observableDictionary.Contains(new KeyValuePair<string, string>(item.Key, "Nope"));
            Assert.False(contains);

            contains = observableDictionary.Contains(new KeyValuePair<string, string>("No Way", item.Value));
            Assert.False(contains);

            contains = observableDictionary.Contains(new KeyValuePair<string, string>("Not Contained", "Not This Item"));
            Assert.False(contains);
        }

        [Fact]
        public void TestContainsKey()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item});

            var containsKey = observableDictionary.ContainsKey(item.Key);
            Assert.True(containsKey);

            containsKey = observableDictionary.ContainsKey("Not Contained");
            Assert.False(containsKey);
        }

        [Fact]
        public void TestClear()
        {
            var item = new KeyValuePair<string, string>("1", "value");
            var item2 = new KeyValuePair<string, string>("2", "another value");
            var item3 = new KeyValuePair<string, string>("3", "yet another value");
            var observableDictionary = PerformObservableDictionarySetup(new List<KeyValuePair<string, string>> {item, item2, item3});

            Assert.Equal(3, observableDictionary.Count);

            var latch = new CountdownEvent(4);

            var collectionChangedActions = new List<NotifyCollectionChangedAction>();
            var propertiesChanged = new List<string>();

            void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs args)
            {
                collectionChangedActions.Add(args.Action);
                latch?.Signal();
            }

            void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
            {
                propertiesChanged.Add(args.PropertyName);
                latch?.Signal();
            }

            observableDictionary.CollectionChanged += CollectionChangedHandler;
            observableDictionary.PropertyChanged += PropertyChangedHandler;

            observableDictionary.Clear();

            latch.Wait();

            Assert.Empty(observableDictionary);
            
            Assert.Single(collectionChangedActions);
            Assert.Equal(NotifyCollectionChangedAction.Reset, collectionChangedActions[0]);

            Assert.Equal(3, propertiesChanged.Count);
            Assert.Contains("Count", propertiesChanged);
            Assert.Contains("Keys", propertiesChanged);
            Assert.Contains("Values", propertiesChanged);
        }
    }
}