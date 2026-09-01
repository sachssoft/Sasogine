namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>Specifies the current editing mode of a vector path tool.</summary>
    public enum VectorPathToolMode
    {
        /// <summary>No vector path editing mode is active.</summary>
        None = 0,

        /// <summary>Selects and manipulates vector path elements.</summary>
        Selection = 1,

        /// <summary>Draws new vector paths and segments.</summary>
        Draw = 2,

        /// <summary>Inserts new nodes or segments into an existing vector path.</summary>
        Insert = 3
    }
}