using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PBase.Test
{
    public class BaseSynchronisedTests
    {
        private class TestSynchronised : BaseSynchronised
        {
            public void Enter()
            {
                SyncRoot.Enter();
            }

            public void Exit()
            {
                SyncRoot.Exit();
            }
        }

        [Fact]
        public void TestBaseSynchronised()
        {
            var timeout = TimeSpan.FromSeconds(15.0);
            SafeLock.Timeout = timeout / 3;

            var sut = new TestSynchronised();

            Task.Run(() =>
            {
                sut.Enter();
                Thread.Sleep(timeout);
                sut.Exit();
            });

            Thread.Sleep(1000);

            Assert.Throws<TimeoutException>(() => sut.Enter());
        }
    }
}
