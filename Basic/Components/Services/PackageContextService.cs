using Sachssoft.Sasogine.Basic.Packages;
using Sachssoft.Sasogine.Packages;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Components.Services
{
    /// <summary>
    /// Manages the current package context and notifies listeners when
    /// the active package changes.
    /// </summary>
    public sealed class PackageContextService<TPackage>
        where TPackage : class, IPackageStorageProvider
    {
        /// <summary>
        /// Occurs when the active package has changed.
        /// </summary>
        public event EventHandler<PackageChangedEventArgs>? PackageChanged;

        /// <summary>
        /// Gets the currently active package.
        /// </summary>
        public TPackage? CurrentPackage { get; private set; }

        /// <summary>
        /// Gets a value indicating whether an active package is available.
        /// </summary>
        [MemberNotNullWhen(true, nameof(CurrentPackage))]
        public bool HasPackage => CurrentPackage != null;

        /// <summary>
        /// Sets the active package.
        /// </summary>
        /// <param name="packageStorageProvider">
        /// The package to activate, or <see langword="null"/> to clear the current package.
        /// </param>
        public void SetPackage(TPackage? packageStorageProvider)
        {
            if (ReferenceEquals(CurrentPackage, packageStorageProvider))
                return;

            var lastPackage = CurrentPackage;
            CurrentPackage = packageStorageProvider;

            PackageChanged?.Invoke(this, new PackageChangedEventArgs(lastPackage, CurrentPackage));
        }
    }
}