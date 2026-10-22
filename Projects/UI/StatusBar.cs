using System;

namespace Sachssoft.Engine.Controls;
/// <summary>Arranges status strips in a single horizontal row.</summary>
public class StatusBar : StackPanel
{
    /// <summary>Creates a horizontal status bar without a local orientation override.</summary>
    public StatusBar() : base(StackOrientation.Horizontal)
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "StatusBar";

    /// <summary>Adds a status strip, reserving its ownership immediately.</summary>
    /// <exception cref = "ArgumentException">The child is not a StatusStrip.</exception>
    public override void Add(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (child is not StatusStrip)
            throw new ArgumentException("StatusBar children must be StatusStrip hosts.", nameof(child));
        base.Add(child);
    }

    /// <summary>Adds a status strip, reserving its ownership immediately.</summary>
    public void Add(StatusStrip statusStrip) => base.Add(statusStrip);

    /// <summary>
    /// ???
    /// </summary>
    public void Add(StatusStrip statusStrip, GridLength length)
    {
        statusStrip.Length = length; // Bequemer Code
        base.Add(statusStrip);
    }
}
