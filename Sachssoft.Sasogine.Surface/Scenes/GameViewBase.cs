using Sachssoft.Sasogine.Engine;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface.Controls;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Surface.Scenes;

public abstract class GameViewBase<TRuntime> : SurfaceSceneBase<TRuntime> where TRuntime : RuntimeBase
{
    private bool _is_loaded = false;
    private bool _is_paused;
    private Grid? _loading_screen;
    private TRuntime _runtime;
    private GameContext _loadContext;

    public event EventHandler? IsPauseChanged;

    public GameViewBase()
    {
    }

    public new TRuntime Runtime
    {
        get => _runtime;
        set
        {
            if (_runtime != value)
            {
                _runtime?.Unload();
                _runtime = value;
                base.Runtime = _runtime;

                if (_runtime != null && _is_loaded)
                {
                    _runtime.Load();
                }
            }
        }
    }

    protected override void OnLoad()
    {
        if (!_is_loaded)
            Runtime?.Load();

        // Achtung! Kein base.OnLoad() !!!
        // Siehe stattdessen RuntimeLoading 

        //OnUIBuilt(CreateContainer());
        _is_loaded = true;
    }

    protected override void OnDrawContent(PresentationContext context)
    {
        if (!IsReady)
            return;

        base.OnDrawContent(context);
    }

    protected virtual void OnLoadProgressStarted() { }

    //protected virtual new Container CreateContainer()
    //{
    //    var container = new Grid();
    //    Widgets.Add(container);
    //    return container;
    //}

    //protected virtual void OnUIBuilt(Container container)
    //{
    //}

    protected virtual void OnLoading(float percent)
    {
    }

    protected virtual void OnReady()
    {
    }

    public bool IsReady => true;

    public bool IsPaused
    {
        get => _is_paused;
        set
        {
            if (_is_paused != value)
            {
                _is_paused = value;
                IsPauseChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
