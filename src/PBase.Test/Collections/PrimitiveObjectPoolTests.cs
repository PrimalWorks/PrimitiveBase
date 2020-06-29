using PBase.Test.Support;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace PBase.Test.Collections
{
    public class PrimitiveObjectPoolTests : BaseUnitTest
    {
        public PrimitiveObjectPoolTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper) {}

        [Fact]
        void TestingGounds()
        {
            //need to check that in use counter is always correct
        }

    }
}
