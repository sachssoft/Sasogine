using Sachssoft.Observables;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Observables
{
    public static class PropertyCategories
    {
        public static readonly PropertyCategory Animation = new("animation", "Animation");
        public static readonly PropertyCategory Transform = new("transform", "Transform");
        public static readonly PropertyCategory Layout = new("layout", "Layout");
        public static readonly PropertyCategory Conditions = new("conditions", "Conditions");
        public static readonly PropertyCategory Behavior = new("behavior", "Behavior");
        public static readonly PropertyCategory Border = new("border", "Border");
        public static readonly PropertyCategory Editor = new("editor", "Editor");
        public static readonly PropertyCategory Appearance = new("appearance", "Appearance");
        public static readonly PropertyCategory Scene = new("scene", "Scene");
        public static readonly PropertyCategory Parallax = new("parallax", "Parallax");
        public static readonly PropertyCategory General = new("general", "General");
        public static readonly PropertyCategory Rendering = new("rendering", "Rendering");
        public static readonly PropertyCategory Geometry = new("geometry", "Geometry");
        public static readonly PropertyCategory World = new("world", "World");
        public static readonly PropertyCategory Local = new("local", "Local");
        public static readonly PropertyCategory Camera = new("camera", "Camera");
    }
}
