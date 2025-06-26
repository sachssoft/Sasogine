using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using sachssoft.Sasogine.Features;
using sachssoft.Sasogine.Surface;
using sachssoft.Sasogine.Surface.Views;
using sachssoft.Sasogine.Surface.Visuals.Controls;
using sachssoft.Sasogine.Utils;

namespace sachssoft.Sasogine.Surface.Views;

public abstract class GameViewBase<TRuntime> : SurfaceViewBase where TRuntime : RuntimeBase
{
    private bool _is_paused;
    private FadeOverlay? _fade_overlay;
    private Grid? _loading_screen;
    private TaskProgressTracker _loading_tracker;

    public event EventHandler? IsPauseChanged;

    public GameViewBase()
    {
        _loading_tracker = CreateLoadingTracker();
    }

    protected abstract TRuntime CreateRuntime();

    public new TRuntime Runtime => (TRuntime)base.Runtime!;

    protected virtual TaskProgressTracker CreateLoadingTracker()
    {
        var tpt = new TaskProgressTracker();

        tpt.Started += (s, e) => OnLoadProgressStarted();
        tpt.TaskFinished += async (s, e) => await LoadingTaskFinished();
        tpt.ProgressChanged += (s, e) => OnLoading(e.Percent);
        tpt.Completed += (s, e) => OnReady();

        return tpt;
    }

    private async Task LoadingTaskFinished()
    {
        await Task.Run(() => RuntimeLoading()).ConfigureAwait(false);
        await FadeOutLoadingScreenAsync();
    }

    protected override void OnLoad()
    {
        base.Runtime = CreateRuntime();

        // Achtung! Kein base.OnLoad() !!!
        // Siehe stattdessen RuntimeLoading 

        OnUIBuilt(CreateContainer());
        BuildOverlay();
    }

    protected override void OnDraw(GameContext context)
    {
        if (!IsReady)
            return;

        base.OnDraw(context);
    }

    protected override void OnDrawAfterGUI(GameContext context)
    {
        base.OnDrawAfterGUI(context);
    }

    protected virtual void RuntimeLoading()
    {
        Runtime?.Load();
    }

    protected TaskProgressTracker LoadingTracker => _loading_tracker;

    public void StartLoadProgress()
    {
        _loading_tracker.Start();
    }

    protected virtual void OnLoadProgressStarted() { }

    protected virtual Container CreateContainer()
    {
        var container = new Grid();
        Widgets.Add(container);
        return container;
    }

    protected virtual void OnUIBuilt(Container container)
    {
    }

    protected virtual void OnLoading(float percent)
    {
    }

    protected virtual void OnReady()
    {
    }

    public bool IsReady => _loading_tracker.IsReady;

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

    // -------------------------
    // Fade-Effekte
    // -------------------------

    // ms
    public int FadeDelay { get; set; } = 100;

    // ms
    public int FadeDuration { get; set; } = 1000;

    protected virtual void OnLoadingScreenBuilt(Container container) { }

    private void BuildOverlay()
    {
        _fade_overlay = new FadeOverlay(Color.Black, 1f);

        _loading_screen = new Grid();
        OnLoadingScreenBuilt(_loading_screen);

        Widgets.Add(_fade_overlay);
        Widgets.Add(_loading_screen);
    }

    private async Task FadeOutLoadingScreenAsync()
    {
        if (_loading_screen is { } content)
        {
            Widgets.Remove(content);
        }

        if (_fade_overlay is { } overlay)
        {
            await overlay.FadeToAsync(0f, FadeDuration);
            Widgets.Remove(overlay);
            _fade_overlay = null;
        }
    }
}
