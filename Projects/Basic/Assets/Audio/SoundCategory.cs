namespace Sachssoft.Sasogine.Assets.Audio
{
    /// <summary>
    /// Specifies the purpose or playback context of a sound asset.
    /// </summary>
    public enum SoundCategory
    {
        /// <summary>
        /// Represents user interface sounds such as buttons, menus,
        /// and interaction feedback.
        /// </summary>
        UI,

        /// <summary>
        /// Represents ambient sounds such as environmental noise,
        /// weather, or background effects.
        /// </summary>
        Ambient,

        /// <summary>
        /// Represents action-related sounds such as weapons,
        /// hits, or explosions.
        /// </summary>
        Action,

        /// <summary>
        /// Represents footsteps and other movement-related sounds.
        /// </summary>
        Footstep,

        /// <summary>
        /// Represents physical impacts, collisions, and object interactions.
        /// </summary>
        Impact,

        /// <summary>
        /// Represents dialogue, speech, or voice lines.
        /// </summary>
        Dialogue,

        /// <summary>
        /// Represents notifications, alerts, achievements,
        /// or other informational sounds.
        /// </summary>
        Notification
    }
}