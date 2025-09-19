/* 
 * © 2024 Tobias Sachs
 * ViewBase
 * 11.07.2024 
*/

using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Diagnostics;
using Sachssoft.Sasogine.Features;
using Sachssoft.Sasogine.Services;

namespace Sachssoft.Sasogine.Surface;

public abstract class ViewBase /*Panel,*/
{
    private FrameCounter _frame_counter;
    private TimeSpan _elapsed_game_time;
    private bool _init;
    internal ISurfaceElement _container = null;
    //internal Widget? ContainerCache = null;
    private bool _is_active;
    private readonly ViewSwitchMode _view_switch_mode;

    public ViewBase(ViewSwitchMode view_switch_mode = ViewSwitchMode.Restart)
    {
        _frame_counter = new();
        _view_switch_mode = view_switch_mode;

        OnInitialize();
        _container = CreateContainer();
    }

    public ISurfaceElement Container { get => _container; }

    protected virtual ISurfaceElement CreateContainer() => throw new NotImplementedException();

    [MaybeNull]
    public SurfaceHost Host
    {
        get
        {
            if (Container == null)
                throw new InvalidOperationException("No surface host");

            return Container.Host;
        }
    }

    //public IGameDebugService? Debug
    //{
    //    get;
    //    protected set;
    //}

    public RuntimeBase? Runtime
    {
        get;
        set;
    }

    public ViewSwitchMode ViewSwitchMode
    {
        get => _view_switch_mode;
        init => _view_switch_mode = value;
    }

    public bool IsActive
    {
        get => _is_active;
        internal set
        {
            if (_is_active != value)
            {
                _is_active = value;
                if (_is_active)
                {
                    OnActivated();
                }
                else
                {
                    OnDeactivated();
                }
            }
        }
    }

    public bool IsLoaded
    {
        get;
        internal set;
    }

    protected void Leave()
    {
        IsActive = false;
        OnLeft();
        OnUnload();
        IsLoaded = false;
    }

    protected virtual void OnLeft()
    {
        IMyGameApp.Current.View.Close(this);
    }

    internal protected virtual void OnActivated() { }
    internal protected virtual void OnDeactivated() { }

    //protected GameRuntimeBase? Runtime
    //{
    //    get;
    //    set;
    //}

    // Später Umstellung

    public virtual ISurfaceElement? Build() => null;

    internal protected virtual void OnOpening()
    {
    }

    internal protected virtual void OnClosing()
    {
    }

    public FrameCounter FrameCounter => _frame_counter;

    public TimeSpan ElapsedGameTime => _elapsed_game_time;

    internal virtual void OnLoadPrepareInternal()
    {
    }

    internal protected virtual void OnInitialize()
    {
    }

    internal protected virtual void OnLoad(GameBaseContext context)
    {
        Runtime?.Load(context);
    }

    internal protected virtual void OnUnload()
    {
        Runtime?.Unload();
    }

    internal protected virtual void OnClientSizeChanged()
    {
    }

    internal virtual bool CanRender(GameFrameContext context)
    {
        return true;
    }

    internal protected virtual void OnUpdate(GameFrameContext context)
    {
        Runtime?.Update(context);
    }

    internal protected virtual void OnDraw(GameFrameContext context)
    {
        Runtime?.Draw(context);
    }

    internal protected virtual void OnDrawAfterGUI(GameFrameContext context)
    {
    }
}