using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PBase.Test
{
    public class BaseNotifiableTests
    {
        private class TestNotifiable : BaseNotifiable
        {
            private bool m_prop;

            public bool Prop
            {
                get
                {
                    return m_prop;
                }
                set
                {
                    Set<bool>(ref m_prop, value);
                }
            }

            public object TestNulls(ref int count)
            {
                object obj = null;
                Set<object>(ref obj, null, "Prop");

                Set<object>(ref obj, new object(), "Prop");

                return obj;
            }

            protected override void Dispose(bool disposing)
            {

            }
        }

        [Fact]
        public void TestBaseNotifiable()
        {
            int count = 0;
            var sut = new TestNotifiable();

            sut.Prop = true;

            Assert.True(sut.Prop);

            sut.PropertyChanged += (s, e) =>
            {
                Assert.Equal("Prop", e.PropertyName);
                count++;
            };

            sut.Prop = false;

            Assert.Equal(1, count);

            sut.Prop = false;

            Assert.Equal(1, count);

            Assert.NotNull(sut.TestNulls(ref count));

            Assert.Equal(2, count);
        }
    }
}
