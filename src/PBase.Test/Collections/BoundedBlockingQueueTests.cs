using PBase.Collections;
using PBase.Test.Support;
using System;
using System.Collections;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace PBase.Test.Collections
{
    public class BoundedBlockingQueueTests : BaseUnitTest
    {
        public BoundedBlockingQueueTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        private class TestQueueItem
        {
            public int Value { get; set; }
        }

        [Fact]
        void TestBoundedBlockingQueueCreation()
        {
            BoundedBlockingQueue<TestQueueItem> bbq = new BoundedBlockingQueue<TestQueueItem>(5);

            Assert.NotNull(bbq);

            Assert.Equal(5, bbq.Size);
            Assert.Equal(0, bbq.Count);
            Assert.NotNull(bbq.QueueSyncRoot);
            Assert.False(bbq.IsReadOnly);

            BoundedBlockingQueue<TestQueueItem> invalidBBQ = null;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                invalidBBQ = new BoundedBlockingQueue<TestQueueItem>(0);
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                invalidBBQ = new BoundedBlockingQueue<TestQueueItem>(-1);
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                invalidBBQ = new BoundedBlockingQueue<TestQueueItem>(-5);
            });

            Assert.Null(invalidBBQ);
        }

        [Fact]
        async void TestBoundedBlockingQueueEnqueueDequeue()
        {
            BoundedBlockingQueue<TestQueueItem> bbq = new BoundedBlockingQueue<TestQueueItem>(2);

            Assert.Equal(0, bbq.Count);
            Assert.Equal(2, bbq.Size);

            Assert.Throws<ArgumentNullException>(() =>
            {
                bbq.Enqueue(null);
            });

            Assert.Equal(0, bbq.Count);

            Assert.False(bbq.TryEnqueue(null));

            Assert.Equal(0, bbq.Count);

            bbq.Enqueue(new TestQueueItem { Value = 100 });
            bbq.Enqueue(new TestQueueItem { Value = 200 });

            Assert.Equal(2, bbq.Count);

            Assert.Throws<TimeoutException>(() => bbq.Enqueue(new TestQueueItem { Value = 300 }, 1000));

            Assert.Equal(2, bbq.Count);

            Assert.False(bbq.TryEnqueue(new TestQueueItem { Value = 300 }, 1000));

            Assert.Equal(2, bbq.Count);

            var item100 = bbq.Dequeue();

            Assert.Equal(2, bbq.Size);
            Assert.Equal(1, bbq.Count);
            Assert.Equal(100, item100.Value);

            bbq.Enqueue(new TestQueueItem { Value = 500 }, 1000);
            Assert.Equal(2, bbq.Count);

            var item200 = bbq.Dequeue();

            Assert.Equal(200, item200.Value);
            Assert.Equal(1, bbq.Count);

            bbq.Enqueue(new TestQueueItem { Value = 400 });

            Task.Run(() =>
            {
                bbq.Enqueue(new TestQueueItem { Value = 600 });
            });

            await Task.Delay(1000); //allow enough time for task to set up

            var item500 = bbq.Dequeue();

            await Task.Delay(1000); //allow enough time for task to enqueue 600

            Assert.Equal(500, item500.Value);
            Assert.Equal(2, bbq.Count); //400 and 600 should be in here

            var item400 = bbq.Dequeue();
            Assert.Equal(1, bbq.Count);
            Assert.Equal(400, item400.Value);

            bool tryDeqRes = bbq.TryDequeue(out var item600);
            Assert.True(tryDeqRes);
            Assert.Equal(0, bbq.Count);
            Assert.Equal(600, item600.Value);

            TestQueueItem itemNull = null;
            Assert.Throws<TimeoutException>(() => itemNull = bbq.Dequeue(500));
            Assert.Null(itemNull);

            TestQueueItem item700 = null;
            Task.Run(() =>
            {
                item700 = bbq.Dequeue();
            });

            await Task.Delay(1000); //allow enough time for task to set up

            bbq.Enqueue(new TestQueueItem { Value = 700 });

            await Task.Delay(1000); //allow enough time for task to dequeue 700

            Assert.Equal(700, item700.Value);
            Assert.Equal(0, bbq.Count);
        }

        [Fact]
        async void TestBoundedBlockingQueueClearDispose()
        {
            BoundedBlockingQueue<TestQueueItem> bbq = new BoundedBlockingQueue<TestQueueItem>(3);

            bbq.TryEnqueue(new TestQueueItem { Value = 100 });
            bbq.Enqueue(new TestQueueItem { Value = 200 });

            Assert.Equal(2, bbq.Count);

            bbq.Enqueue(new TestQueueItem { Value = 300 });

            Task.Run(async () =>
            {
                await Task.Delay(1000); //have to wait as new item will be enqueued immediately after clearing
                bbq.Enqueue(new TestQueueItem { Value = 400 });
            });

            bbq.Clear();

            Assert.Equal(0, bbq.Count);

            var result = bbq.TryDequeue(out var qItem);
            Assert.False(result);
            Assert.Null(qItem);

            await Task.Delay(1500);
            Assert.Equal(1, bbq.Count);
            var item400 = bbq.Dequeue();
            Assert.Equal(400, item400.Value);

            bbq.Dispose();

            Assert.True(bbq.IsDisposed);

            Assert.Throws<ObjectDisposedException>(() => bbq.Count);

            Assert.Throws<ObjectDisposedException>(() => bbq.Size);

            Assert.Throws<ObjectDisposedException>(() => bbq.QueueSyncRoot);

            Assert.Throws<ObjectDisposedException>(() => bbq.Values);

            Assert.Throws<ObjectDisposedException>(() => bbq.IsReadOnly);

            Assert.Throws<ObjectDisposedException>(() => bbq.Enqueue(new TestQueueItem { Value = 1000 }));

            Assert.False(bbq.TryEnqueue(new TestQueueItem { Value = 1000 }));

            Assert.Throws<ObjectDisposedException>(() => bbq.Peek());

            var resTryPeek = bbq.TryPeek(out var itemPeek);
            Assert.False(resTryPeek);
            Assert.Null(itemPeek);

            Assert.Throws<ObjectDisposedException>(() => bbq.Dequeue());

            var resTryDeq = bbq.TryDequeue(out var itemDequeue);
            Assert.False(resTryDeq);
            Assert.Null(itemDequeue);

            Assert.Throws<ObjectDisposedException>(() => bbq.Contains(new TestQueueItem { Value = 1000 }));
            Assert.Throws<ObjectDisposedException>(() => bbq.CopyTo(new TestQueueItem[3], 0));

            Assert.Throws<ObjectDisposedException>(() => bbq.GetEnumerator());
            Assert.Throws<ObjectDisposedException>(() => (bbq as IEnumerable).GetEnumerator());
        }

        [Fact]
        void TestBoundedBlockingQueueAddRemove()
        {
            BoundedBlockingQueue<TestQueueItem> bbq = new BoundedBlockingQueue<TestQueueItem>(1);

            Assert.Equal(0, bbq.Count);

            Assert.Throws<InvalidOperationException>(() => bbq.Add(new TestQueueItem { Value = 100 }));

            Assert.Equal(0, bbq.Count);

            Assert.Throws<InvalidOperationException>(() => bbq.Remove(new TestQueueItem { Value = 100 }));

            Assert.Equal(0, bbq.Count);
        }

        [Fact]
        async void TestBoundedBlockingQueueClearMultipleWaitingThreads()
        {
            BoundedBlockingQueue<TestQueueItem> bbq = new BoundedBlockingQueue<TestQueueItem>(3);

            var item100 = new TestQueueItem { Value = 100 };
            var item200 = new TestQueueItem { Value = 200 };
            var item300 = new TestQueueItem { Value = 300 };
            var item400 = new TestQueueItem { Value = 400 };
            var item500 = new TestQueueItem { Value = 500 };
            var item600 = new TestQueueItem { Value = 600 };

            bbq.Enqueue(item100);
            bbq.Enqueue(item200);
            bbq.Enqueue(item300);

            Assert.Equal(3, bbq.Count);

            var values = bbq.Values;

            Assert.Equal(3, values.Length);
            Assert.Contains(item100, values);
            Assert.Contains(item200, values);
            Assert.Contains(item300, values);
            Assert.Contains(item100, bbq);
            Assert.Contains(item200, bbq);
            Assert.Contains(item300, bbq);

            bool exceptionThrown = false;

            Task.Run(() =>
            {
                try
                {
                    bbq.Enqueue(item400);
                }
                catch
                {
                    exceptionThrown = true;
                }
            });

            Task.Run(() =>
            {
                try
                {
                    bbq.Enqueue(item500);
                }
                catch
                {
                    exceptionThrown = true;
                }
            });

            Task.Run(() =>
            {
                try
                {
                    bbq.Enqueue(item600);
                }
                catch
                {
                    exceptionThrown = true;
                }
            });

            await Task.Delay(1000);

            Assert.False(exceptionThrown);

            bbq.Clear();

            await Task.Delay(1000);

            values = bbq.Values;

            Assert.Equal(3, values.Length);
            Assert.Equal(3, bbq.Count);
            Assert.Contains(item400, values);
            Assert.Contains(item500, values);
            Assert.Contains(item600, values);
            Assert.Contains(item400, bbq);
            Assert.Contains(item500, bbq);
            Assert.Contains(item500, bbq);
        }

        [Fact]
        async void TestBoundedBlockingQueuePeek()
        {
            BoundedBlockingQueue<TestQueueItem> bbq = new BoundedBlockingQueue<TestQueueItem>(1);

            Assert.Equal(0, bbq.Count);

            bool tryPeekEmpty = bbq.TryPeek(out var testQueueItem);

            Assert.False(tryPeekEmpty);
            Assert.Null(testQueueItem);

            TestQueueItem emptyItem = null;

            Assert.Throws<InvalidOperationException>(() => emptyItem = bbq.Peek());

            Assert.Null(emptyItem);

            Assert.Throws<TimeoutException>(() => emptyItem = bbq.Peek(500, true));

            Assert.Null(emptyItem);
            Assert.Equal(0, bbq.Count);

            bbq.Enqueue(new TestQueueItem { Value = 100 });

            Assert.Equal(1, bbq.Count);

            var item100Peek = bbq.Peek();
            Assert.Equal(1, bbq.Count);
            Assert.Equal(100, item100Peek.Value);

            var item100 = bbq.Dequeue();
            Assert.Equal(0, bbq.Count);
            Assert.Equal(100, item100.Value);

            bool exceptionThrown = false;
            TestQueueItem item200 = null;

            Task.Run(() =>
            {
                try
                {
                    item200 = bbq.Peek(5000, true);
                }
                catch
                {
                    exceptionThrown = true;
                }
            });

            await Task.Delay(1500);

            bbq.Enqueue(new TestQueueItem { Value = 200 });

            await Task.Delay(1000);

            Assert.Equal(1, bbq.Count);
            Assert.Equal(200, item200.Value);

            Assert.False(exceptionThrown);

            bool tryPeekRes = bbq.TryPeek(out var item200TryPeek);

            Assert.Equal(1, bbq.Count);
            Assert.True(tryPeekRes);
            Assert.Equal(200, item200.Value);
        }

        [Fact]
        void TestBoundedBlockingQueueCopy()
        {
            BoundedBlockingQueue<TestQueueItem> bbq = new BoundedBlockingQueue<TestQueueItem>(3);

            bbq.Enqueue(new TestQueueItem { Value = 100 });
            bbq.Enqueue(new TestQueueItem { Value = 200 });
            bbq.Enqueue(new TestQueueItem { Value = 300 });

            var values = bbq.Values;

            TestQueueItem[] copyArray = new TestQueueItem[bbq.Size];
            bbq.CopyTo(copyArray, 0);

            foreach (var item in copyArray)
            {
                Assert.Contains(item, bbq);
                Assert.Contains(item, values);
            }
        }
    }
}

#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
