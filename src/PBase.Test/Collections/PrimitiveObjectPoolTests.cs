using PBase.Collections;
using PBase.Test.Support;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PBase.Test.Collections
{
    public class PrimitiveObjectPoolTests : BaseUnitTest
    {
        public PrimitiveObjectPoolTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper) { }

        private class TestObjectPoolItem
        {
            public TestObjectPoolItem() { }
        }

        [Fact]
        void TestPrimitiveObjectPoolsCreation()
        {
            PrimitiveObjectPool<TestObjectPoolItem> primPool = null;
            ResizablePrimitiveObjectPool<TestObjectPoolItem> resizePool = null;
            BlockingPrimitiveObjectPool<TestObjectPoolItem> blockPool = null;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                primPool = new PrimitiveObjectPool<TestObjectPoolItem>(0);
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                resizePool = new ResizablePrimitiveObjectPool<TestObjectPoolItem>(0);
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                blockPool = new BlockingPrimitiveObjectPool<TestObjectPoolItem>(0);
            });

            Assert.Null(primPool);
            Assert.Null(resizePool);
            Assert.Null(blockPool);

            primPool = new PrimitiveObjectPool<TestObjectPoolItem>(1);
            resizePool = new ResizablePrimitiveObjectPool<TestObjectPoolItem>(1);
            blockPool = new BlockingPrimitiveObjectPool<TestObjectPoolItem>(1);

            Assert.NotNull(primPool);
            Assert.NotNull(resizePool);
            Assert.NotNull(blockPool);
        }

        [Fact]
        void TestPrimitiveObjectPoolsDispose()
        {
            PrimitiveObjectPool<TestObjectPoolItem> primPool = new PrimitiveObjectPool<TestObjectPoolItem>(1);
            ResizablePrimitiveObjectPool<TestObjectPoolItem> resizePool = new ResizablePrimitiveObjectPool<TestObjectPoolItem>(1);
            BlockingPrimitiveObjectPool<TestObjectPoolItem> blockPool = new BlockingPrimitiveObjectPool<TestObjectPoolItem>(1);

            primPool.Dispose();
            resizePool.Dispose();
            blockPool.Dispose();

            Assert.Throws<ObjectDisposedException>(() => primPool.InUse);
            Assert.Throws<ObjectDisposedException>(() => primPool.PoolCount);
            Assert.Throws<ObjectDisposedException>(() => primPool.Size);
            Assert.Throws<ObjectDisposedException>(() => primPool.TryObtain(out var item));
            Assert.Throws<ObjectDisposedException>(() => primPool.TryRelease(new TestObjectPoolItem()));
            Assert.Throws<ObjectDisposedException>(() => primPool.Obtain());
            Assert.Throws<ObjectDisposedException>(() => primPool.Release(new TestObjectPoolItem()));

            Assert.Throws<ObjectDisposedException>(() => resizePool.InUse);
            Assert.Throws<ObjectDisposedException>(() => resizePool.PoolCount);
            Assert.Throws<ObjectDisposedException>(() => resizePool.Size);
            Assert.Throws<ObjectDisposedException>(() => resizePool.TryObtain(out var item));
            Assert.Throws<ObjectDisposedException>(() => resizePool.TryRelease(new TestObjectPoolItem()));
            Assert.Throws<ObjectDisposedException>(() => resizePool.Obtain());
            Assert.Throws<ObjectDisposedException>(() => resizePool.Release(new TestObjectPoolItem()));

            Assert.Throws<ObjectDisposedException>(() => blockPool.InUse);
            Assert.Throws<ObjectDisposedException>(() => blockPool.PoolCount);
            Assert.Throws<ObjectDisposedException>(() => blockPool.Size);
            Assert.Throws<ObjectDisposedException>(() => blockPool.TryObtain(out var item));
            Assert.Throws<ObjectDisposedException>(() => blockPool.TryRelease(new TestObjectPoolItem()));
            Assert.Throws<ObjectDisposedException>(() => blockPool.Obtain());
            Assert.Throws<ObjectDisposedException>(() => blockPool.Release(new TestObjectPoolItem()));
        }

        [Fact]
        void TestPrimitiveObjectPoolObtainRelease()
        {
            PrimitiveObjectPool<TestObjectPoolItem> primPool = new PrimitiveObjectPool<TestObjectPoolItem>(3);

            Assert.Equal(3, primPool.Size);
            Assert.Equal(0, primPool.InUse);
            Assert.Equal(3, primPool.PoolCount);

            TestObjectPoolItem test = new TestObjectPoolItem();

            Assert.False(primPool.TryRelease(test));

            Assert.Equal(3, primPool.Size);
            Assert.Equal(0, primPool.InUse);
            Assert.Equal(3, primPool.PoolCount);

            Assert.Throws<InvalidOperationException>(() => primPool.Release(test));

            Assert.Equal(3, primPool.Size);
            Assert.Equal(0, primPool.InUse);
            Assert.Equal(3, primPool.PoolCount);

            TestObjectPoolItem test1;
            TestObjectPoolItem test2;
            TestObjectPoolItem test3;
            TestObjectPoolItem test4;

            Assert.True(primPool.TryObtain(out test1));

            Assert.NotNull(test1);
            Assert.Equal(3, primPool.Size);
            Assert.Equal(1, primPool.InUse);
            Assert.Equal(2, primPool.PoolCount);

            test2 = primPool.Obtain();

            Assert.NotNull(test2);
            Assert.Equal(3, primPool.Size);
            Assert.Equal(2, primPool.InUse);
            Assert.Equal(1, primPool.PoolCount);

            test3 = primPool.Obtain();

            Assert.NotNull(test3);
            Assert.Equal(3, primPool.Size);
            Assert.Equal(3, primPool.InUse);
            Assert.Equal(0, primPool.PoolCount);

            Assert.False(primPool.TryObtain(out test4));

            Assert.Null(test4);
            Assert.Equal(3, primPool.Size);
            Assert.Equal(3, primPool.InUse);
            Assert.Equal(0, primPool.PoolCount);

            Assert.Throws<InvalidOperationException>(() => test4 = primPool.Obtain());

            Assert.Null(test4);
            Assert.Equal(3, primPool.Size);
            Assert.Equal(3, primPool.InUse);
            Assert.Equal(0, primPool.PoolCount);

            Assert.True(primPool.TryRelease(test1));

            Assert.Equal(3, primPool.Size);
            Assert.Equal(2, primPool.InUse);
            Assert.Equal(1, primPool.PoolCount);

            primPool.Release(test1);

            Assert.Equal(3, primPool.Size);
            Assert.Equal(1, primPool.InUse);
            Assert.Equal(2, primPool.PoolCount);

            primPool.Release(test1);

            Assert.Equal(3, primPool.Size);
            Assert.Equal(0, primPool.InUse);
            Assert.Equal(3, primPool.PoolCount);
        }

        [Fact]
        void TestResizablePrimitiveObjectPoolObtainRelease()
        {
            ResizablePrimitiveObjectPool<TestObjectPoolItem> resizePool = new ResizablePrimitiveObjectPool<TestObjectPoolItem>(3);

            Assert.Equal(3, resizePool.Size);
            Assert.Equal(0, resizePool.InUse);
            Assert.Equal(3, resizePool.PoolCount);

            TestObjectPoolItem test = new TestObjectPoolItem();

            Assert.False(resizePool.TryRelease(test));

            Assert.Equal(3, resizePool.Size);
            Assert.Equal(0, resizePool.InUse);
            Assert.Equal(3, resizePool.PoolCount);

            resizePool.Release(test);

            Assert.Equal(4, resizePool.Size);
            Assert.Equal(0, resizePool.InUse);
            Assert.Equal(4, resizePool.PoolCount);

            TestObjectPoolItem test1;
            TestObjectPoolItem test2;
            TestObjectPoolItem test3;
            TestObjectPoolItem test4;
            TestObjectPoolItem test5;

            Assert.True(resizePool.TryObtain(out test1));

            Assert.NotNull(test1);
            Assert.Equal(4, resizePool.Size);
            Assert.Equal(1, resizePool.InUse);
            Assert.Equal(3, resizePool.PoolCount);

            test2 = resizePool.Obtain();

            Assert.NotNull(test2);
            Assert.Equal(4, resizePool.Size);
            Assert.Equal(2, resizePool.InUse);
            Assert.Equal(2, resizePool.PoolCount);

            test3 = resizePool.Obtain();

            Assert.NotNull(test3);
            Assert.Equal(4, resizePool.Size);
            Assert.Equal(3, resizePool.InUse);
            Assert.Equal(1, resizePool.PoolCount);

            test4 = resizePool.Obtain();

            Assert.NotNull(test4);
            Assert.Equal(4, resizePool.Size);
            Assert.Equal(4, resizePool.InUse);
            Assert.Equal(0, resizePool.PoolCount);

            Assert.False(resizePool.TryObtain(out test5));

            Assert.Null(test5);
            Assert.Equal(4, resizePool.Size);
            Assert.Equal(4, resizePool.InUse);
            Assert.Equal(0, resizePool.PoolCount);

            test5 = resizePool.Obtain();

            Assert.NotNull(test5);
            Assert.Equal(5, resizePool.Size);
            Assert.Equal(5, resizePool.InUse);
            Assert.Equal(0, resizePool.PoolCount);

            Assert.True(resizePool.TryRelease(test5));

            Assert.Equal(5, resizePool.Size);
            Assert.Equal(4, resizePool.InUse);
            Assert.Equal(1, resizePool.PoolCount);

            resizePool.Release(test5);

            Assert.Equal(5, resizePool.Size);
            Assert.Equal(3, resizePool.InUse);
            Assert.Equal(2, resizePool.PoolCount);

            resizePool.Release(test5);

            Assert.Equal(5, resizePool.Size);
            Assert.Equal(2, resizePool.InUse);
            Assert.Equal(3, resizePool.PoolCount);

            resizePool.Release(test5);

            Assert.Equal(5, resizePool.Size);
            Assert.Equal(1, resizePool.InUse);
            Assert.Equal(4, resizePool.PoolCount);

            resizePool.Release(test5);

            Assert.Equal(5, resizePool.Size);
            Assert.Equal(0, resizePool.InUse);
            Assert.Equal(5, resizePool.PoolCount);

            resizePool.Release(test5);

            Assert.Equal(6, resizePool.Size);
            Assert.Equal(0, resizePool.InUse);
            Assert.Equal(6, resizePool.PoolCount);
        }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        [Fact]
        async void TestBlockingPrimitiveObjectPoolObtainRelease()
        {
            BlockingPrimitiveObjectPool<TestObjectPoolItem> blockPool = new BlockingPrimitiveObjectPool<TestObjectPoolItem>(3);

            Assert.Equal(3, blockPool.Size);
            Assert.Equal(0, blockPool.InUse);
            Assert.Equal(3, blockPool.PoolCount);

            TestObjectPoolItem test = new TestObjectPoolItem();

            Assert.False(blockPool.TryRelease(test));

            Assert.Equal(3, blockPool.Size);
            Assert.Equal(0, blockPool.InUse);
            Assert.Equal(3, blockPool.PoolCount);

            Task.Run(() =>
            {
                blockPool.Release(test);
            });

            await Task.Delay(500);

            Assert.Equal(3, blockPool.Size);
            Assert.Equal(0, blockPool.InUse);
            Assert.Equal(3, blockPool.PoolCount);

            var newTest = blockPool.Obtain();

            await Task.Delay(500);

            Assert.NotNull(newTest);
            Assert.Equal(3, blockPool.Size);
            Assert.Equal(0, blockPool.InUse);
            Assert.Equal(3, blockPool.PoolCount);

            TestObjectPoolItem test1;
            TestObjectPoolItem test2;
            TestObjectPoolItem test3;
            TestObjectPoolItem test4;
            TestObjectPoolItem test5 = null;

            Assert.True(blockPool.TryObtain(out test1));

            Assert.NotNull(test1);
            Assert.Equal(3, blockPool.Size);
            Assert.Equal(1, blockPool.InUse);
            Assert.Equal(2, blockPool.PoolCount);

            test2 = blockPool.Obtain();

            Assert.NotNull(test2);
            Assert.Equal(3, blockPool.Size);
            Assert.Equal(2, blockPool.InUse);
            Assert.Equal(1, blockPool.PoolCount);

            test3 = blockPool.Obtain();

            Assert.NotNull(test3);
            Assert.Equal(3, blockPool.Size);
            Assert.Equal(3, blockPool.InUse);
            Assert.Equal(0, blockPool.PoolCount);

            Assert.False(blockPool.TryObtain(out test4));

            Assert.Null(test4);
            Assert.Equal(3, blockPool.Size);
            Assert.Equal(3, blockPool.InUse);
            Assert.Equal(0, blockPool.PoolCount);

            Task.Run(() =>
            {
                test4 = blockPool.Obtain();
            });

            Task.Run(() =>
            {
                test5 = blockPool.Obtain();
            });

            await Task.Delay(500);

            Assert.Null(test4);
            Assert.Null(test5);

            blockPool.Release(test1);

            await Task.Delay(500);

            Assert.Equal(3, blockPool.Size);
            Assert.Equal(3, blockPool.InUse);
            Assert.Equal(0, blockPool.PoolCount);

            Assert.True((test4 != null) ^ (test5 != null));

            blockPool.Release(test1);

            await Task.Delay(1000);

            Assert.NotNull(test4);
            Assert.NotNull(test5);

            Assert.Equal(3, blockPool.Size);
            Assert.Equal(3, blockPool.InUse);
            Assert.Equal(0, blockPool.PoolCount);

            Assert.True(blockPool.TryRelease(test1));

            Assert.Equal(3, blockPool.Size);
            Assert.Equal(2, blockPool.InUse);
            Assert.Equal(1, blockPool.PoolCount);

            blockPool.Release(test1);

            Assert.Equal(3, blockPool.Size);
            Assert.Equal(1, blockPool.InUse);
            Assert.Equal(2, blockPool.PoolCount);

            blockPool.Release(test1);

            Assert.Equal(3, blockPool.Size);
            Assert.Equal(0, blockPool.InUse);
            Assert.Equal(3, blockPool.PoolCount);

            bool dispose1 = false;
            bool dispose2 = false;

            Task.Run(() =>
            {
                try
                {
                    blockPool.Release(test1);
                }
                catch (ObjectDisposedException ex)
                {
                    dispose1 = true;
                }
            });

            Task.Run(() =>
            {
                try
                {
                    blockPool.Release(test2);
                }
                catch (ObjectDisposedException ex)
                {
                    dispose2 = true;
                }
            });

            await Task.Delay(500);

            Assert.Equal(3, blockPool.Size);
            Assert.Equal(0, blockPool.InUse);
            Assert.Equal(3, blockPool.PoolCount);

            blockPool.Dispose();

            await Task.Delay(500);

            Assert.True(dispose1);
            Assert.True(dispose2);
        }

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

    }
}
