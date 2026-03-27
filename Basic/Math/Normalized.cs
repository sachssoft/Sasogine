using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Math
{
    public readonly struct Normalized : IEquatable<Normalized>, IComparable<Normalized>
    {
        private readonly float _v;

        public Normalized(float value)
        {
            _v = value < 0f ? 0f : (value > 1f ? 1f : value);
        }

        // Implizit von/zu float
        public static implicit operator Normalized(float value) => new Normalized(value);
        public static implicit operator float(Normalized n) => n._v;

        // Operatoren
        public static Normalized operator +(Normalized a, Normalized b) => new Normalized(a._v + b._v);
        public static Normalized operator -(Normalized a, Normalized b) => new Normalized(a._v - b._v);
        public static Normalized operator *(Normalized a, Normalized b) => new Normalized(a._v * b._v);
        public static Normalized operator /(Normalized a, Normalized b) => new Normalized(a._v / b._v);

        public static bool operator ==(Normalized a, Normalized b) => a._v == b._v;
        public static bool operator !=(Normalized a, Normalized b) => a._v != b._v;
        public static bool operator <(Normalized a, Normalized b) => a._v < b._v;
        public static bool operator >(Normalized a, Normalized b) => a._v > b._v;
        public static bool operator <=(Normalized a, Normalized b) => a._v <= b._v;
        public static bool operator >=(Normalized a, Normalized b) => a._v >= b._v;

        public override string ToString() => _v.ToString("0.##");
        public bool Equals(Normalized other) => _v == other._v;
        public override bool Equals(object obj) => obj is Normalized n && Equals(n);
        public override int GetHashCode() => _v.GetHashCode();
        public int CompareTo(Normalized other) => _v.CompareTo(other._v);
    }
}
