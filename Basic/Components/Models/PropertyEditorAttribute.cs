using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Sasogine.Basic.Components.Models
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertyEditorAttribute : Attribute
    {
        public PropertyEditorAttribute()
        {
        }
    }
}