using Microsoft.Extensions.Logging;
using PBase.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace PBase.Test.Support
{
    public abstract class BaseUnitTest
    {
        public BaseUnitTest(ITestOutputHelper testOutputHelper)
        {
            var factory = new LoggerFactory();
            factory.AddProvider(new UnitTestLoggerProvider(testOutputHelper));
            PLog.AssignLoggerFactory(factory);
        }
    }
}
