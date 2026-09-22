using System;

namespace Sachssoft.Engine.Components.Models;

/// <summary>
/// Marks a property as available for editing by a property editor.
/// </summary>
/// <remarks>
/// This attribute can be applied only to properties, cannot be applied
/// multiple times to the same property, and is inherited by derived types.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class PropertyEditorAttribute : Attribute
{
}