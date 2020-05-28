using PBase.Collections;
using PBase.Test.Support;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

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

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        [Fact]
        void TestBoundedBlockingQueueCreation()
        {
            BoundedBlockingQueue<TestQueueItem> bbq = new BoundedBlockingQueue<TestQueueItem>(5);

            Assert.NotNull(bbq);

            Assert.Equal(5, bbq.Size);
            Assert.Equal(0, bbq.Count);
        }

        [Fact]
        async void TestBoundedBlockingQueueEnqueue()
        {
            BoundedBlockingQueue<TestQueueItem> bbq = new BoundedBlockingQueue<TestQueueItem>(2);
            bbq.Enqueue(new TestQueueItem { Value = 100 });
            bbq.Enqueue(new TestQueueItem { Value = 200 });

            Assert.Equal(2, bbq.Size);
            Assert.Equal(2, bbq.Count);

            Assert.Throws<TimeoutException>(() => bbq.Enqueue(new TestQueueItem { Value = 300 }, 1000));

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

            await Task.Delay(5000);

            var item500 = bbq.Dequeue();

            await Task.Delay(1000);

            Assert.Equal(500, item500.Value);
            Assert.Equal(2, bbq.Count); //400 and 600 should be in here

            var item400 = bbq.Dequeue();
            Assert.Equal(1, bbq.Count);
            Assert.Equal(400, item400.Value);
            
            var item600 = bbq.Dequeue();
            Assert.Equal(0, bbq.Count);
            Assert.Equal(600, item600.Value);
            


        }

#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        [Fact]
        void TestingGround()
        {

        }
    }
}
