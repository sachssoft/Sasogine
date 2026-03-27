using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Scenes;

public abstract class SurfaceSceneBase<TRuntime> : SceneBase<TRuntime>, IModalHost, IWindowHost
    where TRuntime : RuntimeBase
{
    public SurfaceSceneBase()
        : base()
    {
    }

    public new Desktop Host => (Desktop)base.Host!;

    #region IModalHost

    IEnumerable<IModalContent> IModalHost.Modals =>
        ((IModalHost)Host).Modals;

    Rectangle IModalHost.Bounds =>
        ((IModalHost)Host).Bounds;

    void IModalHost.AddModal(IModalContent modal) =>
        ((IModalHost)Host).AddModal(modal);

    void IModalHost.RemoveModal(IModalContent modal) =>
        ((IModalHost)Host).RemoveModal(modal);

    #endregion

    #region IWindowHost

    IEnumerable<IWindowContent> IWindowHost.Windows =>
        ((IWindowHost)Host).Windows;

    Rectangle IWindowHost.Bounds =>
        ((IWindowHost)Host).Bounds;

    void IWindowHost.AddWindow(IWindowContent window) =>
        ((IWindowHost)Host).AddWindow(window);

    void IWindowHost.RemoveWindow(IWindowContent window) =>
        ((IWindowHost)Host).RemoveWindow(window);

    #endregion
}
