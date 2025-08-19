using System;
using System.IO;

namespace Sachssoft.Sasogine.Containers
{
    /// <summary>
    /// Abstract base class for lazily wrapping package entries that provide
    /// a stream to load a resource of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the resource to be loaded.</typeparam>
    public abstract class PackageEntryWrapper<T> : LazyWrapper<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageEntryWrapper{T}"/> class,
        /// wrapping a deferred stream provider from the specified package.
        /// This enables lazy loading of the resource to minimize memory usage.
        /// </summary>
        /// <param name="package">The package containing the resource data.</param>
        /// <param name="streamProvider">
        /// A function that takes the package and returns a <see cref="Stream"/> with the resource data,
        /// or <c>null</c> if the resource cannot be provided.
        /// </param>
        protected PackageEntryWrapper(PackageBase package, Func<PackageBase, Stream?> streamProvider)
            : base((package, streamProvider), typeof(PackageBase))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageEntryWrapper{T}"/> class
        /// wrapping the provided content of a specified source type.
        /// </summary>
        /// <param name="content">The wrapped content to be lazily transformed.</param>
        /// <param name="sourceType">The type of the wrapped content.</param>
        protected PackageEntryWrapper(object? content, Type sourceType)
            : base(content, sourceType)
        {
        }


        /// <summary>
        /// Determines whether the specified <see cref="Type"/> is <see cref="PackageBase"/> 
        /// or a subclass thereof.
        /// </summary>
        /// <param name="sourceType">The type to check.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="sourceType"/> is <see cref="PackageBase"/> or derived from it; otherwise, <c>false</c>.
        /// </returns>
        protected bool IsPackageType(Type sourceType)
        {
            return sourceType.IsAssignableTo(typeof(PackageBase));
        }

        /// <summary>
        /// Retrieves a stream from the wrapped content by invoking the stored stream provider.
        /// </summary>
        /// <param name="wrapContent">Expected to be a tuple of <see cref="PackageBase"/> and <see cref="Func{PackageBase, Stream?}"/>.</param>
        /// <returns>The stream provided by the stream provider function, or <c>null</c> if no stream is available.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="wrapContent"/> is not of the expected type,
        /// if the package is <c>null</c>, or if the package is not open and thus cannot provide a stream.
        /// </exception>
        protected Stream? OpenWrappedStream(object? wrapContent)
        {
            if (wrapContent is (PackageBase package, Func<PackageBase, Stream?> func))
            {
                if (package == null)
                    throw new InvalidOperationException("Package is null.");

                if (!package.IsOpen)
                    throw new InvalidOperationException("The package is not open and cannot provide a stream.");

                return func(package);
            }

            throw new InvalidOperationException("Wrapped content is not of expected type.");
        }

    }
}
