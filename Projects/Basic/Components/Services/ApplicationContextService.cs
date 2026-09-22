using System;

namespace Sachssoft.Engine.Components.Services;

/// <summary>
/// Provides the application context as a component service.
/// </summary>
public class ApplicationContextService : IComponentService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationContextService"/> class.
    /// </summary>
    /// <param name="context">The application context provided by this service.</param>
    public ApplicationContextService(IApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Context = context;
    }

    /// <summary>
    /// Gets the application context provided by this service.
    /// </summary>
    public IApplicationContext Context { get; }
}