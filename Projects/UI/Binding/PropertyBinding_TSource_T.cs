using System;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls.Binding;
/// <summary>Connects typed delegates without reflection, property discovery or expression compilation.</summary>
/// <typeparam name = "TSource">The model type.</typeparam>
/// <typeparam name = "T">The property value type.</typeparam>
public sealed class PropertyBinding<TSource, T> : IFrameBinding, IDisposable where TSource : INotifyPropertyChanged
{
    private readonly TSource _source;
    private readonly Element _target;
    private readonly string _sourceName, _targetName;
    private readonly Func<TSource, T> _readSource;
    private readonly Action<T> _writeTarget;
    private readonly Func<T>? _readTarget;
    private readonly Action<TSource, T>? _writeSource;
    private bool _sourceDirty = true, _targetDirty, _updating;
    /// <summary>Creates a one-way binding; supplying both reverse delegates enables two-way binding.</summary>
    public PropertyBinding(TSource source, string sourceProperty, Func<TSource, T> readSource, Element target, string targetProperty, Action<T> writeTarget, Func<T>? readTarget = null, Action<TSource, T>? writeSource = null)
    {
        _source = source;
        _target = target;
        _sourceName = sourceProperty;
        _targetName = targetProperty;
        _readSource = readSource;
        _writeTarget = writeTarget;
        _readTarget = readTarget;
        _writeSource = writeSource;
        if ((readTarget is null) != (writeSource is null))
            throw new ArgumentException("Both reverse delegates are required for two-way binding.");
        source.PropertyChanged += SourceChanged;
        target.PropertyChanged += TargetChanged;
    }

    private void SourceChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (!_updating && (string.IsNullOrEmpty(args.PropertyName) || args.PropertyName == _sourceName))
            _sourceDirty = true;
    }

    private void TargetChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (!_updating && _writeSource is not null && args.PropertyName == _targetName)
            _targetDirty = true;
    }

    /// <summary>Applies pending notifications in a stable order on the UI thread.</summary>
    public void Update()
    {
        _updating = true;
        try
        {
            if (_sourceDirty)
            {
                _sourceDirty = false;
                _targetDirty = false;
                _writeTarget(_readSource(_source));
            }
            else if (_targetDirty)
            {
                _targetDirty = false;
                _writeSource!(_source, _readTarget!());
                _writeTarget(_readSource(_source));
            }
        }
        finally
        {
            _updating = false;
        }
    }

    /// <summary>Unsubscribes this binding when its owning screen is removed.</summary>
    public void Dispose()
    {
        _source.PropertyChanged -= SourceChanged;
        _target.PropertyChanged -= TargetChanged;
    }
}
