using System;

namespace Sachssoft.Sasogine.Components.Models
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertyEditorAttribute : Attribute
    {
        public PropertyEditorAttribute()
        {
        }
    }
}