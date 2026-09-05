namespace Sachssoft.Sasogine.Assets.Audio
{
    /// <summary>
    /// Specifies the purpose or playback context of a music asset.
    /// </summary>
    public enum MusicCategory
    {
        /// <summary>
        /// Represents background music typically used during gameplay.
        /// </summary>
        Background,

        /// <summary>
        /// Represents music used for menus, title screens, or similar
        /// non-gameplay contexts.
        /// </summary>
        Menu,

        /// <summary>
        /// Represents music used for events, cutscenes, or other
        /// temporary gameplay situations.
        /// </summary>
        Event
    }
}