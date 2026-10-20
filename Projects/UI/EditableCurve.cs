using Sachssoft.Engine.Gameplay.Curves;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sachssoft.Engine.Controls;
/// <summary>Editable CurveBase with fixed endpoint times, ordered points and cached shape-preserving slopes.</summary>
public sealed class EditableCurve : CurveBase, System.ComponentModel.INotifyPropertyChanged
{
    /// <summary>Minimum distance between normalized point times.</summary>
    public const float MinimumSpacing = 0.0001f;
    private readonly List<CurvePoint> _points = new();
    private readonly ReadOnlyCollection<CurvePoint> _view;
    private double[] _slopes = Array.Empty<double>();
    private int _revision, _slopeRevision = -1;
    private CurveInterpolation _interpolation = CurveInterpolation.Monotone;
    /// <summary>Creates a straight 0-to-1 curve.</summary>
    public EditableCurve()
    {
        _points.Add(new(0, 0));
        _points.Add(new(1, 1));
        _view = _points.AsReadOnly();
    }

    /// <summary>Creates a validated copy of ordered points.</summary>
    public EditableCurve(IEnumerable<CurvePoint> points) : this() => ReplacePoints(points);
    /// <summary>Gets points without exposing mutable list storage.</summary>
    public IReadOnlyList<CurvePoint> Points => _view;
    /// <summary>Gets the revision changed by edits and interpolation changes.</summary>
    public int Revision => _revision;

    /// <summary>Gets or sets interpolation.</summary>
    public CurveInterpolation Interpolation
    {
        get => _interpolation;
        set
        {
            if (value is not (CurveInterpolation.Step or CurveInterpolation.Linear or CurveInterpolation.Monotone))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_interpolation == value)
                return;
            _interpolation = value;
            Changed();
        }
    }

    private static readonly System.ComponentModel.PropertyChangedEventArgs RevisionArgs = new(nameof(Revision));
    /// <summary>Occurs after points or interpolation change the source revision.</summary>
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    private void Changed()
    {
        unchecked
        {
            _revision++;
        }

        _slopeRevision = unchecked(_revision - 1);
        PropertyChanged?.Invoke(this, RevisionArgs);
    }

    private static void ValidatePoint(CurvePoint point)
    {
        if (!float.IsFinite(point.X) || !float.IsFinite(point.Y) || point.X < 0 || point.X > 1)
            throw new ArgumentOutOfRangeException(nameof(point));
    }

    /// <summary>Replaces points atomically after validating all input.</summary>
    public void ReplacePoints(IEnumerable<CurvePoint> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        var next = new List<CurvePoint>(points);
        if (next.Count < 2 || next[0].X != 0 || next[^1].X != 1)
            throw new ArgumentException("A curve needs endpoints at X=0 and X=1.", nameof(points));
        for (int i = 0; i < next.Count; i++)
        {
            ValidatePoint(next[i]);
            if (i > 0 && (double)next[i].X - next[i - 1].X < MinimumSpacing * .999)
                throw new ArgumentException("Point times must increase with minimum spacing.", nameof(points));
        }

        bool same = next.Count == _points.Count;
        if (same)
            for (int i = 0; i < next.Count; i++)
                if (next[i] != _points[i])
                {
                    same = false;
                    break;
                }

        if (same)
            return;
        _points.Clear();
        _points.AddRange(next);
        Changed();
    }

    /// <summary>Adds an interior point and returns its sorted index.</summary>
    public int AddPoint(CurvePoint point)
    {
        ValidatePoint(point);
        int index = 1;
        while (index < _points.Count && _points[index].X < point.X)
            index++;
        if (index >= _points.Count || (double)point.X - _points[index - 1].X < MinimumSpacing * .999 || (double)_points[index].X - point.X < MinimumSpacing * .999)
            throw new ArgumentException("Point time duplicates or touches a neighboring point.", nameof(point));
        _points.Insert(index, point);
        Changed();
        return index;
    }

    /// <summary>Changes a point without permitting reordering or moving endpoint times.</summary>
    public void SetPoint(int index, CurvePoint point)
    {
        if (index < 0 || index >= _points.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        ValidatePoint(point);
        if (index == 0 && point.X != 0 || index == _points.Count - 1 && point.X != 1 || index > 0 && (double)point.X - _points[index - 1].X < MinimumSpacing * .999 || index < _points.Count - 1 && (double)_points[index + 1].X - point.X < MinimumSpacing * .999)
            throw new ArgumentException("Point time violates endpoint or ordering constraints.", nameof(point));
        if (_points[index] == point)
            return;
        _points[index] = point;
        Changed();
    }

    /// <summary>Removes an interior point, retaining both endpoints.</summary>
    public void RemovePoint(int index)
    {
        if (index <= 0 || index >= _points.Count - 1)
            throw new ArgumentOutOfRangeException(nameof(index));
        _points.RemoveAt(index);
        Changed();
    }

    /// <inheritdoc/>
    public override float GetValue(float value)
    {
        if (!float.IsFinite(value))
            throw new ArgumentOutOfRangeException(nameof(value));
        if (value <= 0)
            return _points[0].Y;
        if (value >= 1)
            return _points[^1].Y;
        int lo = 0, hi = _points.Count - 1;
        while (hi - lo > 1)
        {
            int mid = (lo + hi) / 2;
            if (_points[mid].X <= value)
                lo = mid;
            else
                hi = mid;
        }

        var a = _points[lo];
        var b = _points[lo + 1];
        if (value == a.X)
            return a.Y;
        if (_interpolation == CurveInterpolation.Step)
            return a.Y;
        double h = (double)b.X - a.X, t = ((double)value - a.X) / h;
        if (_interpolation == CurveInterpolation.Linear)
            return (float)((1 - t) * a.Y + t * b.Y);
        EnsureSlopes();
        double t2 = t * t, t3 = t2 * t;
        double result = (2 * t3 - 3 * t2 + 1) * a.Y + (t3 - 2 * t2 + t) * h * _slopes[lo] + (-2 * t3 + 3 * t2) * b.Y + (t3 - t2) * h * _slopes[lo + 1];
        return (float)Math.Clamp(result, Math.Min(a.Y, b.Y), Math.Max(a.Y, b.Y));
    }

    private double H(int i) => (double)_points[i + 1].X - _points[i].X;
    private double D(int i) => ((double)_points[i + 1].Y - _points[i].Y) / H(i);
    private static double EndSlope(double h0, double h1, double d0, double d1)
    {
        double slope = ((2 * h0 + h1) * d0 - h0 * d1) / (h0 + h1);
        if (Math.Sign(slope) != Math.Sign(d0))
            return 0;
        if (Math.Sign(d0) != Math.Sign(d1) && Math.Abs(slope) > Math.Abs(3 * d0))
            return 3 * d0;
        return slope;
    }

    private void EnsureSlopes()
    {
        if (_slopeRevision == _revision)
            return;
        int n = _points.Count;
        if (_slopes.Length < n)
            _slopes = new double[n];
        if (n == 2)
        {
            _slopes[0] = _slopes[1] = D(0);
        }
        else
        {
            _slopes[0] = EndSlope(H(0), H(1), D(0), D(1));
            _slopes[n - 1] = EndSlope(H(n - 2), H(n - 3), D(n - 2), D(n - 3));
            for (int i = 1; i < n - 1; i++)
            {
                double previous = D(i - 1), next = D(i);
                if (previous == 0 || next == 0 || Math.Sign(previous) != Math.Sign(next))
                    _slopes[i] = 0;
                else
                {
                    double w1 = 2 * H(i) + H(i - 1), w2 = H(i) + 2 * H(i - 1);
                    _slopes[i] = (w1 + w2) / (w1 / previous + w2 / next);
                }
            }
        }

        _slopeRevision = _revision;
    }
}
