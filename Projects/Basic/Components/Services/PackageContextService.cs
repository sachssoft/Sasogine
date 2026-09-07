using Sachssoft.Sasogine.Packages;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Components.Services
{
    /// <summary>
    /// Manages the current package context and notifies listeners when
    /// the active package changes.
    /// </summary>
    public class PackageContextService<TPackage> : IComponentService
        where TPackage : class, IPackageStorageProvider
    {
        /// <summary>
        /// Occurs before the active package changes.
        /// </summary>
        public event EventHandler<PackageChangedEventArgs>? PackageChanging;

        /// <summary>
        /// Occurs after the active package has changed.
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
        /// The package to activate, or <see langword="null"/> to clear the
        /// current package.
        /// </param>
        public void SetPackage(TPackage? packageStorageProvider)
        {
            if (ReferenceEquals(CurrentPackage, packageStorageProvider))
                return;

            var lastPackage = CurrentPackage;
            var eventArgs = new PackageChangedEventArgs(lastPackage, packageStorageProvider);

            OnPackageChanging(eventArgs);

            CurrentPackage = packageStorageProvider;

            OnPackageChanged(eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="PackageChanging"/> event.
        /// </summary>
        /// <param name="e">
        /// The event data describing the package change.
        /// </param>
        private void OnPackageChanging(PackageChangedEventArgs e)
        {
            PackageChanging?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="PackageChanged"/> event.
        /// </summary>
        /// <param name="e">
        /// The event data describing the package change.
        /// </param>
        private void OnPackageChanged(PackageChangedEventArgs e)
        {
            PackageChanged?.Invoke(this, e);
        }
    }
}