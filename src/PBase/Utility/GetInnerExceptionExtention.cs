using System;
using System.Collections.Generic;
using System.Text;

namespace PBase.Utility
{
    public static class GetInnerExceptionExtention
    {
        /// <summary>
        ///     Extension method that returns an enumerable of inner exceptions.
        /// </summary>
        /// <param name="e">The exception from which to get all inner exceptions.</param>
        /// <returns>
        ///     An enumerable of the inner exceptions of exception e.
        /// </returns>
        public static IEnumerable<Exception> GetInnerExceptions(this Exception e)
        {
            var innerExceptions = new List<Exception>();
            var innerException = e.InnerException;

            while (innerException != null)
            {
                innerExceptions.Add(innerException);
                innerException = innerException.InnerException;
            }

            return innerExceptions;
        }
    }
}
