using Sachssoft.Sasogine.Basic.Packages;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Components.Services
{
    /// <summary>
    /// Provides access to the currently loaded package.
    /// </summary>
    public sealed class PackageContextService<TPackage>
        where TPackage : class, IPackageStorageProvider
    {
        /// <summary>
        /// Occurs when the current package changes.
        /// </summary>
        public event EventHandler? PackageChanged;


        /// <summary>
        /// Gets the currently assigned package.
        /// </summary>
        public TPackage? CurrentPackage { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a package is currently assigned.
        /// </summary>
        [MemberNotNullWhen(true, nameof(CurrentPackage))]
        public bool HasPackage => CurrentPackage != null;

        /// <summary>
        /// Sets the current package.
        /// </summary>
        /// <param name="packageStorageProvider">
        /// Package storage provider to assign.
        /// </param>
        public void SetPackage(TPackage? packageStorageProvider)
        {
            if (ReferenceEquals(CurrentPackage, packageStorageProvider))
                return;

            CurrentPackage = packageStorageProvider;

            PackageChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}