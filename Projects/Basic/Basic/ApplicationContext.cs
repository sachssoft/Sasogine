using System;

namespace Sachssoft.Sasogine;

/// <summary>
/// Provides the default application context used by engine components.
/// </summary>
public class ApplicationContext : IApplicationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationContext"/> class.
    /// </summary>
    /// <param name="application">
    /// The game application associated with this context.
    /// </param>
    public ApplicationContext(GameApplicationBase application)
    {
        ArgumentNullException.ThrowIfNull(application);

        Application = application;
    }

    /// <inheritdoc/>
    public GameApplicationBase Application { get; }
}