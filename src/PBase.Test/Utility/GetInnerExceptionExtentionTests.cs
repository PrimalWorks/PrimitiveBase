using System;
using System.Linq;
using PBase.Utility;
using Xunit;

namespace PBase.Test.Utility
{
    internal class TestException : Exception
    {
        public TestException(string message) : base(message)
        {
        }

        public TestException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class GetInnerExceptionExtentionTests
    {
        [Fact]
        public void TestGetInnerExceptions()
        {
            var innermostException = new TestException("Oops!");
            var veryInnerException = new TestException("Whoops!", innermostException);
            var innerException = new TestException("Oh No!", veryInnerException);
            var slightlyInnerException = new TestException("Oops!", innerException);

            //GetInnerExceptions should not include the outermost exception in the returned enumberable.
            var outermostException = new TestException("Oops!", slightlyInnerException);

            var oopsExceptions = outermostException.GetInnerExceptions().Where(e => e.Message == "Oops!");

            Assert.Equal(2, oopsExceptions.Count());
        }
    }
}