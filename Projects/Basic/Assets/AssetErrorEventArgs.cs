using System;

namespace Sachssoft.Sasogine.Assets;

/// <summary>
/// Provides data for the <see cref="AssetBase{T, TDefinition}.Error"/> event.
/// </summary>
public sealed class AssetErrorEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssetErrorEventArgs"/> class.
    /// </summary>
    /// <param name="exception">
    /// The exception associated with the asset error.
    /// </param>
    public AssetErrorEventArgs(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        Exception = exception;
    }

    /// <summary>
    /// Gets the exception associated with the asset error.
    /// </summary>
    public Exception Exception { get; }
}