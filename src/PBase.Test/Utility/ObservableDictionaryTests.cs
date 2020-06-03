using System;
using System.Threading;
using PBase.Utility;
using Xunit;

namespace PBase.Test.Utility
{
    public class ObservableDictionaryTests
    {
       [Fact]
        public void TestObservableDictionaryAdd()
        {
            ObservableDictionary<string, string> observableDictionary;
            observableDictionary = new ObservableDictionary<string, string> {{"First", "The first value"}, {"Second", "The second value"}};

            var collectionChanged = 0;
            var propertyChanged = 0;

            observableDictionary.CollectionChanged += (sender, args) =>
            {
                collectionChanged++;
            };

            observableDictionary.PropertyChanged += (sender, args) =>
            {
                propertyChanged++;
            };

            observableDictionary.Add("Third", "A third value");

            Thread.Sleep(5000);

            Assert.Equal(1, collectionChanged);
            Assert.Equal(3, propertyChanged);
        }

        [Fact]
        public void TestObservableDictionaryRemove()
        {
            ObservableDictionary<string, string> observableDictionary;
            observableDictionary = new ObservableDictionary<string, string> {{"First", "The first value"}, {"Second", "The second value"}};

            var collectionChanged = 0;
            var propertyChanged = 0;

            observableDictionary.CollectionChanged += (sender, args) =>
            {
                collectionChanged++;
            };

            observableDictionary.PropertyChanged += (sender, args) =>
            {
                propertyChanged++;
            };

            observableDictionary.Remove("First");

            Thread.Sleep(5000);

            Assert.Equal(1, collectionChanged);
            Assert.Equal(3, propertyChanged);
        }

        [Fact]
        public void TestObservableDictionaryUpdate()
        {
            ObservableDictionary<string, string> observableDictionary;
            observableDictionary = new ObservableDictionary<string, string> {{"First", "The first value"}, {"Second", "The second value"}};

            var collectionChanged = 0;
            var propertyChanged = 0;

            observableDictionary.CollectionChanged += (sender, args) =>
            {
                collectionChanged++;
            };

            observableDictionary.PropertyChanged += (sender, args) =>
            {
                propertyChanged++;
            };

            observableDictionary["Second"] = "A new second value";

            Thread.Sleep(5000);


            Assert.Equal(1, collectionChanged);
            Assert.Equal(0, propertyChanged);
        }
    }
}