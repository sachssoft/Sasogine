namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>Positions a control within its available layout slot.</summary>
public enum ControlAlignment
{
    /// <summary>Aligns with the start of the slot.</summary>
    Start,
    /// <summary>Centers in the slot.</summary>
    Center,
    /// <summary>Aligns with the end of the slot.</summary>
    End,
    /// <summary>Uses all available space unless an explicit size is given.</summary>
    Stretch
}
