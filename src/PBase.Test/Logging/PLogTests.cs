using Microsoft.Extensions.Logging;
using PBase.Logging;
using PBase.Test.Support;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace PBase.Test.Logging
{
    public class PLogTests : BaseUnitTest
    {
        public static AssertLogger TestLog = new AssertLogger();
        public PLogTests()
            : base(TestLog)
        {
            // Here we are providing our own TestOutputHelper so we can check the correct string is logged
        }

        [Fact]
        public void TestPLogInitialisation()
        {
            Assert.NotNull(PLog.LoggerFactory);
            Assert.NotNull(PLog.Logger);

            Assert.NotNull(PLog.CreateLogger("UnitTestAnotherLogger"));
            Assert.NotNull(PLog.CreateLogger<PLogTests>());
        }

        [Fact]
        public void TestPLogLogging()
        {
            var testlevel = "Critical";
            var testex = new ArgumentException("ARGEX");
            var testevent = new EventId(123);

            TestLog.Clear();
            PLog.LogCritical("TEST1");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST1");

            TestLog.Clear();
            PLog.LogCritical("TEST2 {0}", "PARAM2");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST2", "PARAM2");

            TestLog.Clear();
            PLog.LogCritical(testex, "TEST3 {0}", "PARAM3");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST3", "PARAM3", "ARGEX");

            TestLog.Clear();
            PLog.LogCritical(testevent, "TEST4");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST4", "[123]");

            TestLog.Clear();
            PLog.LogCritical(testevent, testex, "TEST4");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST4", "[123]", "ARGEX");

            testlevel = "Debug";
            testex = new ArgumentException("ARGEX");
            testevent = new EventId(123);

            TestLog.Clear();
            PLog.LogDebug("TEST1");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST1");

            TestLog.Clear();
            PLog.LogDebug("TEST2 {0}", "PARAM2");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST2", "PARAM2");

            TestLog.Clear();
            PLog.LogDebug(testex, "TEST3 {0}", "PARAM3");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST3", "PARAM3", "ARGEX");

            TestLog.Clear();
            PLog.LogDebug(testevent, "TEST4");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST4", "[123]");

            TestLog.Clear();
            PLog.LogDebug(testevent, testex, "TEST4");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST4", "[123]", "ARGEX");

            testlevel = "Error";
            testex = new ArgumentException("ARGEX");
            testevent = new EventId(123);

            TestLog.Clear();
            PLog.LogError("TEST1");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST1");

            TestLog.Clear();
            PLog.LogError("TEST2 {0}", "PARAM2");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST2", "PARAM2");

            TestLog.Clear();
            PLog.LogError(testex, "TEST3 {0}", "PARAM3");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST3", "PARAM3", "ARGEX");

            TestLog.Clear();
            PLog.LogError(testevent, "TEST4");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST4", "[123]");

            TestLog.Clear();
            PLog.LogError(testevent, testex, "TEST4");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST4", "[123]", "ARGEX");

            testlevel = "Information";
            testex = new ArgumentException("ARGEX");
            testevent = new EventId(123);

            TestLog.Clear();
            PLog.LogInformation("TEST1");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST1");

            TestLog.Clear();
            PLog.LogInformation("TEST2 {0}", "PARAM2");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST2", "PARAM2");

            TestLog.Clear();
            PLog.LogInformation(testex, "TEST3 {0}", "PARAM3");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST3", "PARAM3", "ARGEX");

            TestLog.Clear();
            PLog.LogInformation(testevent, "TEST4");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST4", "[123]");

            TestLog.Clear();
            PLog.LogInformation(testevent, testex, "TEST4");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST4", "[123]", "ARGEX");

            testlevel = "Trace";
            testex = new ArgumentException("ARGEX");
            testevent = new EventId(123);

            TestLog.Clear();
            PLog.LogTrace("TEST1");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST1");

            TestLog.Clear();
            PLog.LogTrace("TEST2 {0}", "PARAM2");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST2", "PARAM2");

            TestLog.Clear();
            PLog.LogTrace(testex, "TEST3 {0}", "PARAM3");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST3", "PARAM3", "ARGEX");

            TestLog.Clear();
            PLog.LogTrace(testevent, "TEST4");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST4", "[123]");

            TestLog.Clear();
            PLog.LogTrace(testevent, testex, "TEST4");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST4", "[123]", "ARGEX");

            testlevel = "Warning";
            testex = new ArgumentException("ARGEX");
            testevent = new EventId(123);

            TestLog.Clear();
            PLog.LogWarning("TEST1");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST1");

            TestLog.Clear();
            PLog.LogWarning("TEST2 {0}", "PARAM2");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST2", "PARAM2");

            TestLog.Clear();
            PLog.LogWarning(testex, "TEST3 {0}", "PARAM3");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST3", "PARAM3", "ARGEX");

            TestLog.Clear();
            PLog.LogWarning(testevent, "TEST4");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST4", "[123]");

            TestLog.Clear();
            PLog.LogWarning(testevent, testex, "TEST4");
            TestLog.AssertLogMatch("PBase", testlevel, "TEST4", "[123]", "ARGEX");
        }

        public class PLogNullTest
        {
            [Fact]
            public void TestWhenLoggerIsNUll()
            {
                var testex = new ArgumentException("ARGEX");
                var testevent = new EventId(123);

                PLog.LogCritical("TEST1");
                PLog.LogCritical("TEST2 {0}", "PARAM2");
                PLog.LogCritical(testex, "TEST3 {0}", "PARAM3");
                PLog.LogCritical(testevent, "TEST4");
                PLog.LogCritical(testevent, testex, "TEST4");

                testex = new ArgumentException("ARGEX");
                testevent = new EventId(123);

                PLog.LogDebug("TEST1");
                PLog.LogDebug("TEST2 {0}", "PARAM2");
                PLog.LogDebug(testex, "TEST3 {0}", "PARAM3");
                PLog.LogDebug(testevent, "TEST4");
                PLog.LogDebug(testevent, testex, "TEST4");

                testex = new ArgumentException("ARGEX");
                testevent = new EventId(123);

                PLog.LogError("TEST1");
                PLog.LogError("TEST2 {0}", "PARAM2");
                PLog.LogError(testex, "TEST3 {0}", "PARAM3");
                PLog.LogError(testevent, "TEST4");
                PLog.LogError(testevent, testex, "TEST4");

                testex = new ArgumentException("ARGEX");
                testevent = new EventId(123);

                PLog.LogInformation("TEST1");
                PLog.LogInformation("TEST2 {0}", "PARAM2");
                PLog.LogInformation(testex, "TEST3 {0}", "PARAM3");
                PLog.LogInformation(testevent, "TEST4");
                PLog.LogInformation(testevent, testex, "TEST4");

                testex = new ArgumentException("ARGEX");
                testevent = new EventId(123);

                PLog.LogTrace("TEST1");
                PLog.LogTrace("TEST2 {0}", "PARAM2");
                PLog.LogTrace(testex, "TEST3 {0}", "PARAM3");
                PLog.LogTrace(testevent, "TEST4");
                PLog.LogTrace(testevent, testex, "TEST4");

                testex = new ArgumentException("ARGEX");
                testevent = new EventId(123);

                PLog.LogWarning("TEST1");
                PLog.LogWarning("TEST2 {0}", "PARAM2");
                PLog.LogWarning(testex, "TEST3 {0}", "PARAM3");
                PLog.LogWarning(testevent, "TEST4");
                PLog.LogWarning(testevent, testex, "TEST4");
            }
        }

        public class AssertLogger : ITestOutputHelper
        {
            private StringBuilder m_lines = new StringBuilder();

            public void WriteLine(string message)
            {
                m_lines.Append(message);
            }

            public void WriteLine(string format, params object[] args)
            {
                m_lines.Append(string.Format(format, args));
            }

            public void Clear()
            {
                m_lines.Clear();
            }

            public void AssertLogMatch(params string[] args)
            {
                foreach(var arg in args)
                {
                    Assert.Contains(arg, m_lines.ToString());
                }
            }
        }
    }
}
