using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Controls.Layouts;
internal sealed class GridAxisLayout
{
    private GridLength[] _lengths = Array.Empty<GridLength>();
    private float[] _minimum = Array.Empty<float>(), _maximum = Array.Empty<float>();
    private bool[] _fixed = Array.Empty<bool>();
    internal float[] Sizes { get; private set; } = Array.Empty<float>();
    internal int Count => Sizes.Length;

    internal void Configure<T>(IList<T> definitions)
        where T : GridDimension
    {
        int count = Math.Max(1, definitions.Count);
        if (Sizes.Length != count)
        {
            Sizes = new float[count];
            _lengths = new GridLength[count];
            _minimum = new float[count];
            _maximum = new float[count];
            _fixed = new bool[count];
        }

        for (int i = 0; i < count; i++)
        {
            GridDimension? d = definitions.Count == 0 ? null : definitions[i];
            float min = d?.Minimum ?? 0, max = d?.Maximum ?? float.PositiveInfinity;
            if (!float.IsFinite(min) || min < 0 || float.IsNaN(max) || max < min)
                throw new InvalidOperationException("Invalid grid row or column limits.");
            _minimum[i] = min;
            _maximum[i] = max;
            _lengths[i] = d?.Length ?? GridLength.Proportion();
            Sizes[i] = _lengths[i].Unit == GridUnitType.Pixel ? Math.Clamp(_lengths[i].Value, min, max) : min;
        }
    }

    internal float Total(float gap)
    {
        float total = gap * Math.Max(0, Count - 1);
        for (int i = 0; i < Count; i++)
            total += Sizes[i];
        return total;
    }

    internal float Offset(int start, float gap)
    {
        float total = 0;
        for (int i = 0; i < start; i++)
            total += Sizes[i] + gap;
        return total;
    }

    internal float Span(int start, int span, float gap)
    {
        float total = gap * (span - 1);
        for (int i = start; i < start + span; i++)
            total += Sizes[i];
        return total;
    }

    internal bool HasAuto(int start, int span)
    {
        for (int i = start; i < start + span; i++)
            if (_lengths[i].Unit == GridUnitType.Auto)
                return true;
        return false;
    }

    internal void ResolveProportions(float available, float gap)
    {
        if (!float.IsFinite(available))
            return;
        double remaining = Math.Max(0, available - gap * Math.Max(0, Count - 1));
        for (int i = 0; i < Count; i++)
        {
            _fixed[i] = _lengths[i].Unit != GridUnitType.Proportion;
            if (_fixed[i])
                remaining -= Sizes[i];
        }

        for (int pass = 0; pass <= Count; pass++)
        {
            double weight = 0;
            for (int i = 0; i < Count; i++)
                if (!_fixed[i])
                    weight += _lengths[i].Value;
            bool clamped = false;
            double budget = remaining;
            for (int i = 0; i < Count; i++)
                if (!_fixed[i])
                {
                    double target = weight > 0 ? Math.Max(0, budget) * _lengths[i].Value / weight : 0;
                    if (target < _minimum[i] || target > _maximum[i])
                    {
                        Sizes[i] = (float)Math.Clamp(target, _minimum[i], _maximum[i]);
                        remaining -= Sizes[i];
                        _fixed[i] = true;
                        clamped = true;
                    }
                }

            if (clamped)
                continue;
            for (int i = 0; i < Count; i++)
                if (!_fixed[i])
                    Sizes[i] = (float)(weight > 0 ? Math.Max(0, remaining) * _lengths[i].Value / weight : 0);
            break;
        }
    }

    internal void Grow(int start, int span, float desired, float gap, bool unbounded)
    {
        double extra = desired - Span(start, span, gap);
        if (extra <= 0)
            return;
        for (int pass = 0; pass < span && extra > 0.001; pass++)
        {
            int growable = 0;
            for (int i = start; i < start + span; i++)
                if ((_lengths[i].Unit == GridUnitType.Auto || unbounded && _lengths[i].Unit == GridUnitType.Proportion) && Sizes[i] < _maximum[i])
                    growable++;
            if (growable == 0)
                return;
            double share = extra / growable;
            for (int i = start; i < start + span; i++)
                if ((_lengths[i].Unit == GridUnitType.Auto || unbounded && _lengths[i].Unit == GridUnitType.Proportion) && Sizes[i] < _maximum[i])
                {
                    float added = (float)Math.Min(share, _maximum[i] - Sizes[i]);
                    Sizes[i] += added;
                    extra -= added;
                }
        }
    }
}
