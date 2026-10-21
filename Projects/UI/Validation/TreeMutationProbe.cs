using System;
using Sachssoft.Engine.Controls;

internal sealed class TreeMutationProbe : Control
{
    internal Action? Updating;
    internal int Updates;
    protected override void OnUpdate(TimeSpan elapsed)
    {
        Updates++;
        Updating?.Invoke();
    }
}
