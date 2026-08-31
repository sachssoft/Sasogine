namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides access to a <see cref="DisposeManager"/> used for managing
    /// the lifetime of disposable resources.
    /// </summary>
    public interface IDisposeManagerProvider
    {
        /// <summary>
        /// Gets the dispose manager responsible for managing disposable resources.
        /// </summary>
        DisposeManager DisposeManager { get; }
    }
}