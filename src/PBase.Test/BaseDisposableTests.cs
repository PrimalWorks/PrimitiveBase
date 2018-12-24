using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PBase.Test
{
    public class BaseDisposableTests
    {
        private class TestDisposable : BaseDisposable
        {
            public bool ShouldThrow { get; set; }
            protected override void Dispose(bool disposing)
            {
                // Simulate taking time to dispose
                Thread.Sleep(5000);

                if (ShouldThrow)
                {
                    throw new Exception("Exception During Disposal");
                }
            }
        }

        [Fact]
        public void TestBaseDisposable()
        {
            var sut = new TestDisposable();

            Assert.False(sut.IsDisposed);
            Assert.False(sut.IsDisposing);

            Task.Run(() =>
            {
                sut.Dispose();
            });

            Thread.Sleep(1000);

            Assert.False(sut.IsDisposed);
            Assert.True(sut.IsDisposing);

            Thread.Sleep(5000);

            Assert.True(sut.IsDisposed);

            // Dispose when already disposed
            sut.Dispose();

            // Dispose with exception
            var sut2 = new TestDisposable();
            sut2.ShouldThrow = true;
            sut2.Dispose();

        }
    }
}
