using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PBase.Test
{
    public class SafeLockTests
    {
        [Fact]
        public void TestSafeLock()
        {
            var timeout = TimeSpan.FromSeconds(15.0);
            SafeLock.Timeout = timeout / 3;

            Assert.Equal((timeout / 3), SafeLock.Timeout);

            var safeLock = new SafeLock();

            Assert.True(safeLock.TryEnter());
            Assert.True(safeLock.TryEnter());

            safeLock.Exit();
            safeLock.Exit();

            Task.Run(() =>
            {
                using (safeLock.Enter())
                {
                    Thread.Sleep(timeout);
                }
            });

            Thread.Sleep(1000);

            Assert.False(safeLock.TryEnter());

            Assert.Throws<TimeoutException>(()=> safeLock.Enter());

            bool threwException = false;

            Task.Run(() =>
            {
                try
                {
                    using (safeLock.Enter())
                    {
                        using (safeLock.Enter())
                        {
                            Thread.Sleep(timeout);
                        }
                    }
                }
                catch
                {
                    threwException = true;
                }
            });

            Thread.Sleep(timeout);

            Assert.False(threwException);

            Assert.ThrowsAny<Exception>(() => safeLock.Exit());

            bool pulsed = false;

            Task.Run(() =>
            {
                using (IWaitable sync = safeLock.Enter() as IWaitable)
                {
                    sync.Wait((int)timeout.TotalMilliseconds);

                    pulsed = true;
                }
            });

            Thread.Sleep(timeout / 3);

            using (IWaitable sync = safeLock.Enter() as IWaitable)
            {
                sync.PulseAll();
            }

            Thread.Sleep(1000);

            Assert.True(pulsed);
        }
    }
}
