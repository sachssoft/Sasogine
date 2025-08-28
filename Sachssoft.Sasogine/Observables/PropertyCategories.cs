using Sachssoft.Observables;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Observables
{
    public static class PropertyCategories
    {
        public static readonly PropertyCategory Animation = new("animation", "Animation");
        public static readonly PropertyCategory Transform = new("transform", "Transform");

        public static readonly HashSet<PropertyCategory> Categories = new()
        {
            Animation,
            Transform
        };
    }
}
