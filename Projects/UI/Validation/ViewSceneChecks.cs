using System;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Scenes;

/// <summary>Checks camera-free scene contracts without allocating a graphics device.</summary>
internal static class ViewSceneChecks
{
    /// <summary>Checks factories, lifecycle guards, disposal and input argument validation.</summary>
    /// <param name="check">The assertion callback.</param>
    /// <param name="layout">The shared test layout context.</param>
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        using var scene = new ViewSceneProbe(layout.DefaultFont);
        IScene contract = scene;
        check(!scene.IsLoaded && !scene.IsPersistent && scene.ViewCount == 1 && !contract.RequiresCamera,
            "View scenes start unloaded with one camera-free view.");
        Expect<InvalidOperationException>(() => _ = scene.Workspace, check);
        Expect<InvalidOperationException>(scene.Load, check);
        Expect<NotSupportedException>(() => contract.CreateCamera(null!, 0), check);
        Expect<NotSupportedException>(() => contract.CreateDefaultMaterial(null!), check);
        Expect<ArgumentNullException>(() => scene.Enter(null!), check);
        Expect<ArgumentNullException>(() => scene.Update(null!), check);
        Expect<ArgumentNullException>(() => scene.Draw(null!), check);
        scene.Exit();
        scene.Unload();
        ((IApplicationExitAware)scene).OnApplicationExited();
        check(!scene.IsLoaded, "Exiting and unloading an uninitialized scene are harmless.");
        scene.Dispose();
        scene.Dispose();
        Expect<ObjectDisposedException>(scene.Load, check);
        Expect<ObjectDisposedException>(() => scene.Enter(null!), check);
        Expect<ObjectDisposedException>(() => scene.Update(null!), check);
        Expect<ObjectDisposedException>(() => scene.Draw(null!), check);
    }

    /// <summary>Checks that an action throws the expected exception.</summary>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="check">The assertion callback.</param>
    private static void Expect<TException>(Action action, Action<bool, string> check)
        where TException : Exception
    {
        bool caught = false;
        try
        {
            action();
        }
        catch (TException)
        {
            caught = true;
        }
        check(caught, $"Expected {typeof(TException).Name}.");
    }
}
