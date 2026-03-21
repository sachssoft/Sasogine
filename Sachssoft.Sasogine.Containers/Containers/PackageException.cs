using System;

namespace Sachssoft.Sasogine.Containers
{
    /// <summary>
    /// Represents errors that occur when working with packages.
    /// </summary>
    public class PackageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageException"/> class.
        /// </summary>
        public PackageException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PackageException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageException"/> class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception that caused this exception.</param>
        public PackageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
