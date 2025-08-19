using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Visuals.Regions
{
    /// <summary>
    /// Wie die Texturregion in das Ziel-Rechteck eingepasst wird.
    /// </summary>
    public enum StretchMode
    {
        /// <summary>
        /// Einfach gestreckt, sodass das Zielrechteck exakt ausgefüllt wird.
        /// </summary>
        Fill,

        /// <summary>
        /// Skaliert proportional, sodass die komplette Textur sichtbar ist,
        /// aber ggf. Ränder (Letterboxing) entstehen.
        /// </summary>
        Uniform,

        /// <summary>
        /// Skaliert proportional, sodass das Zielrechteck komplett gefüllt wird,
        /// aber ggf. Teile abgeschnitten werden (Crop).
        /// </summary>
        UniformToFill,

        /// <summary>
        /// Zeichnet ohne Skalierung in Originalgröße.
        /// </summary>
        None
    }

}
