namespace Sachssoft.Engine.Controls;

/// <summary>Defines how a Carousel changes its visible page.</summary>
public enum CarouselTransition
{
    /// <summary>Switches immediately without animation.</summary>
    None,
    /// <summary>Moves the previous and incoming pages along the carousel axis.</summary>
    Slide,
    /// <summary>Cross-fades the previous and incoming pages.</summary>
    Fade
}
