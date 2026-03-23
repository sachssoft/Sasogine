namespace Sachssoft.Sasogine.Assets.Audio
{
    /// <summary>
    /// Bestimmt den Zweck der Musik im Spiel.
    /// Dient der Steuerung von Playback, Looping und Mixer-Logik.
    /// </summary>
    public enum MusicCategory
    {
        /// <summary>
        /// Standard-Level-Hintergrundmusik.
        /// Wird normalerweise geloopt und als Hauptmusik gemischt.
        /// </summary>
        Background,

        /// <summary>
        /// Musik für Menüs, Startbildschirm oder Pausenmenü.
        /// Wird separat gesteuert, kann eigenständige Lautstärke haben.
        /// </summary>
        Menu,

        /// <summary>
        /// Kurztracks für Events, Zwischensequenzen oder spezielle Szenen.
        /// Normalerweise nicht geloopt und hat Priorität über Background.
        /// </summary>
        Event
    }
}
