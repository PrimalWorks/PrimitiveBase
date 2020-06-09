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
        [Fact]
        public void TestObservableConcurrentDictionaryAdd()
        {
            var latch = new CountdownEvent(4);

            var collectionChangedActions = new List<NotifyCollectionChangedAction>();
            var propertiesChanged = new List<string>();

            NotifyCollectionChangedEventHandler collectionChangedHandler = (sender, args) =>
            {
                collectionChangedActions.Add(args.Action);
                latch.Signal();
            };

            PropertyChangedEventHandler propertyChangedHandler = (sender, args) =>
            {
                propertiesChanged.Add(args.PropertyName);
                latch.Signal();
            };

            var observableDictionary =
                new ObservableConcurrentDictionary<string, string>(collectionChangedHandler, propertyChangedHandler);

            observableDictionary.Add("1", "value");

            latch.Wait();

            Assert.Equal(1, collectionChangedActions.Count);
            Assert.Equal(NotifyCollectionChangedAction.Add, collectionChangedActions[0]);

            Assert.Equal(3, propertiesChanged.Count);
            Assert.True(propertiesChanged.Contains("Count"));
            Assert.True(propertiesChanged.Contains("Keys"));
            Assert.True(propertiesChanged.Contains("Values"));

            latch.Dispose();
        }

        [Fact]
        public void TestObservableConcurrentDictionaryTryAdd()
        {
            var latch = new CountdownEvent(4);

            var collectionChangedActions = new List<NotifyCollectionChangedAction>();
            var propertiesChanged = new List<string>();

            NotifyCollectionChangedEventHandler collectionChangedHandler = (sender, args) =>
            {
                collectionChangedActions.Add(args.Action);
                latch.Signal();
            };

            PropertyChangedEventHandler propertyChangedHandler = (sender, args) =>
            {
                propertiesChanged.Add(args.PropertyName);
                latch.Signal();
            };

            var observableDictionary =
                new ObservableConcurrentDictionary<string, string>(collectionChangedHandler, propertyChangedHandler);

            var success = observableDictionary.TryAdd("1", "value");

            latch.Wait();

            Assert.True(success);

            Assert.Equal(1, collectionChangedActions.Count);
            Assert.Equal(NotifyCollectionChangedAction.Add, collectionChangedActions[0]);

            Assert.Equal(3, propertiesChanged.Count);
            Assert.True(propertiesChanged.Contains("Count"));
            Assert.True(propertiesChanged.Contains("Keys"));
            Assert.True(propertiesChanged.Contains("Values"));

            latch.Dispose();
        }

        [Fact]
        public void TestObservableConcurrentDictionaryAddWithNullKey()
        {
            var observableDictionary = new ObservableConcurrentDictionary<string, string>();
            Assert.Throws<System.ArgumentNullException>(() => observableDictionary.Add(null, "value"));
        }

        [Fact]
        public void TestObservableConcurrentDictionaryTryAddWithNullKey()
        {
            var observableDictionary = new ObservableConcurrentDictionary<string, string>();
            Assert.Throws<System.ArgumentNullException>(() => observableDictionary.TryAdd(null, "value"));
        }
    }
}