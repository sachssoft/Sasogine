using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Assets.Audio
{
    /// <summary>
    /// Bestimmt den Zweck eines Sounds im Spiel.
    /// Dient der Steuerung von Lautstärke, Priorität und Mixer-Kanal.
    /// </summary>
    public enum SoundCategory
    {
        /// <summary>
        /// Benutzeroberfläche: Buttons, Menüs, Feedback
        /// </summary>
        UI,

        /// <summary>
        /// Hintergrundgeräusche: Wind, Tiere, Umgebungs-SFX
        /// </summary>
        Ambient,

        /// <summary>
        /// Action-Sounds: Waffen, Treffer, Explosionen
        /// </summary>
        Action,

        /// <summary>
        /// Fußschritte oder Bewegungsgeräusche
        /// </summary>
        Footstep,

        /// <summary>
        /// Physische Kollisionen, Objekte, Impacts
        /// </summary>
        Impact,

        /// <summary>
        /// Sprach-Sounds oder Voice Lines
        /// </summary>
        Dialogue,

        /// <summary>
        /// Hinweise, Achievements oder Alerts
        /// </summary>
        Notification
    }
}
