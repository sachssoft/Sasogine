namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Drawable Node: Nodes mit grafischer Darstellung, die im Draw-Loop berücksichtigt werden.
    /// Nur Nodes mit einer sichtbaren Komponente sollten implementieren.
    /// </summary>
    public interface IDrawableNode : INode
    {
        /// <summary>
        /// Zeichnet die Node in den aktuellen GameContext.
        /// </summary>
        /// <param name="gameContext">Aktueller Spiel-Kontext für Draw/Rendering.</param>
        void Draw(GameContext gameContext);
    }
}