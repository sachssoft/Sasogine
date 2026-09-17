using System;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines a context-aware factory for creating, attaching and releasing
/// engine objects from definitions.
/// </summary>
public interface IDefinitionBindingFactory<in TDefinition, TObject, in TContext>
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
    where TContext : class
{
    TObject CreateInstance(TDefinition definition, TContext context);

    void AttachInstance(
        TDefinition definition,
        TObject instance,
        TContext context);

    void ReleaseInstance(
        TDefinition definition,
        TObject instance,
        TContext context);
}
