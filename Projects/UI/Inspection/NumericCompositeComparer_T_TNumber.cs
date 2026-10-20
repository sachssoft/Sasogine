using System.Numerics;

namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>Compares described numeric fields without boxing structs lacking IEquatable.</summary>
internal sealed class NumericCompositeComparer<T, TNumber> : IEqualityComparer<T> where T : struct where TNumber : struct, INumber<TNumber>
{
    private readonly NumericValueComponent<T, TNumber>[] _components;
    internal NumericCompositeComparer(NumericValueComponent<T, TNumber>[] components) => _components = components;
    public bool Equals(T left, T right)
    {
        for (int i = 0; i < _components.Length; i++)
            if (!EqualityComparer<TNumber>.Default.Equals(_components[i].Read(left), _components[i].Read(right)))
                return false;
        return true;
    }

    public int GetHashCode(T value)
    {
        var hash = new HashCode();
        for (int i = 0; i < _components.Length; i++)
            hash.Add(_components[i].Read(value));
        return hash.ToHashCode();
    }
}
