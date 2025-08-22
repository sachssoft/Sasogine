using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Interactions;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface;

public sealed class ViewManager
{
    private readonly Dictionary<Type, (Func<ViewBase> Factory, ViewBase? Instance)> _views = new();
    private readonly SurfaceHost _host;

    private Type? _default_view_type;
    private Action<ViewBase>? _default_view_init;

    private GameContext? _game_context;
    private bool _renderable;

    public ViewManager(SurfaceHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public bool IsRenderable => _renderable;

    public void Load()
    {
        if (_host.View == null)
        {
            if (_default_view_type == null)
                throw new GameException("No default view was set.");

            OpenInternal(_default_view_type, _default_view_init);
        }
    }

    public void Update(GameTime time, Action<GameContext> on)
    {
        var view = _host.View;

        if (view == null)
        {
            _renderable = false;
            _game_context?.Dispose();
            _game_context = null;
            return;
        }

        _game_context?.Dispose();
        _game_context = new GameContext(view, time);

        _renderable = view.CanRender(_game_context);

        if (_renderable)
        {
            view.OnUpdate(_game_context);
            on(_game_context);
        }
    }

    public void Draw(GameTime time, Action<GameContext> on_before_surface, Action<GameContext> on_after_surface)
    {
        if (!_renderable || _game_context == null || _host.View == null)
            return;

        var view = _host.View;

        view.OnDraw(_game_context);
        on_before_surface(_game_context);

        if (_game_context.IsUIVisibled)
        {
            _host.Render();
        }

        on_after_surface(_game_context);
        view.OnDrawAfterGUI(_game_context);

        _game_context.Dispose();
        _game_context = null;
    }

    public void Register<TView>() where TView : ViewBase, new()
        => RegisterView(() => new TView());

    public void RegisterView<TView>(Func<TView> factory) where TView : ViewBase
    {
        var type = typeof(TView);
        if (_views.ContainsKey(type))
            throw new InvalidOperationException($"View '{type.Name}' is already registered.");

        _views[type] = (() => factory(), null);
    }

    public void SetDefault<TView>(Action<TView>? init = null) where TView : ViewBase
    {
        _default_view_type = typeof(TView);
        _default_view_init = init != null ? v => init((TView)v) : null;
    }

    public void Open<TView>(Action<TView>? init = null) where TView : ViewBase
        => OpenInternal(typeof(TView), init != null ? v => init((TView)v) : null);

    public void Close<TView>() where TView : ViewBase
        => CloseInternal(typeof(TView), _default_view_type, _default_view_init);

    public void Close(ViewBase view)
    {
        if (_views.TryGetValue(view.GetType(), out var item) && item.Instance == view)
        {
            CloseInternal(view.GetType(), _default_view_type, _default_view_init);
        }
        else
        {
            throw new InvalidOperationException("The given view is not the current instance.");
        }
    }

    public void Close<TView, TNextView>(Action<TNextView>? next_init = null)
        where TView : ViewBase
        where TNextView : ViewBase
    {
        CloseInternal(typeof(TView), typeof(TNextView),
            next_init != null ? v => next_init((TNextView)v) : null);
    }

    public void Switch<TView>(Action<TView>? init = null) where TView : ViewBase
        => SwitchInternal(typeof(TView), init != null ? v => init((TView)v) : null);

    public bool IsOpen<TView>() where TView : ViewBase
        => _views.TryGetValue(typeof(TView), out var item) && item.Instance != null;

    public bool IsActive<TView>() where TView : ViewBase
        => _host.View is TView;

    private void CloseInternal(Type close_type, Type? next_type, Action<ViewBase>? next_init)
    {
        if (!_views.TryGetValue(close_type, out var item) || item.Instance == null)
            return;

        var view = item.Instance;

        if (_host.View == view)
            _host.View = null;

        if (view.IsLoaded)
        {
            view.OnUnload();
            view.IsLoaded = false;
        }

        view.IsActive = false;
        _views[close_type] = (item.Factory, null);

        if (next_type != null)
            SwitchInternal(next_type, next_init);
    }

    private void SwitchInternal(Type type, Action<ViewBase>? init)
    {
        if (!_views.TryGetValue(type, out var item))
            throw new InvalidOperationException($"View '{type.Name}' is not registered.");

        var view = item.Instance ?? item.Factory();
        var old_view = _host.View;

        if (old_view != null)
        {
            old_view.IsActive = false;

            if (old_view.ViewSwitchMode == ViewSwitchMode.Restart && old_view.IsLoaded)
            {
                old_view.OnUnload();
                old_view.IsLoaded = false;
            }

            if (old_view.ViewSwitchMode == ViewSwitchMode.Restart)
            {
                _views[old_view.GetType()] = (_views[old_view.GetType()].Factory, null);
            }
        }

        // Erst speichern, dann aktivieren
        _views[type] = (item.Factory, view);
        _host.View = view;

        if (!view.IsLoaded && view.ViewSwitchMode == ViewSwitchMode.Restart)
        {
            view.OnLoadPrepareInternal();
            view.OnLoad();
            view.IsLoaded = true;
        }

        init?.Invoke(view);
        view.IsActive = true;
    }


    private void OpenInternal(Type type, Action<ViewBase>? init)
    {
        if (!_views.TryGetValue(type, out var item))
            throw new InvalidOperationException($"View '{type.Name}' is not registered.");

        if (item.Instance != null)
            throw new InvalidOperationException($"View '{type.Name}' is already open.");

        var view = item.Factory();
        _views[type] = (item.Factory, view);
        _host.View = view;

        // NEU: Ladebedingung überarbeitet
        if (!view.IsLoaded)
        {
            view.OnLoadPrepareInternal();
            view.OnLoad();
            view.IsLoaded = true;
        }

        init?.Invoke(view);
        view.IsActive = true;
    }


}
