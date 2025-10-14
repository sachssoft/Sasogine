using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Core
{
    public readonly struct SignedNormalized : IEquatable<SignedNormalized>, IComparable<SignedNormalized>
    {
        private readonly float _v;

        public SignedNormalized(float value)
        {
            _v = value < -1f ? -1f : (value > 1f ? 1f : value);
        }

        // Implizit von/zu float
        public static implicit operator SignedNormalized(float value) => new SignedNormalized(value);
        public static implicit operator float(SignedNormalized n) => n._v;

        // Operatoren
        public static SignedNormalized operator +(SignedNormalized a, SignedNormalized b) => new SignedNormalized(a._v + b._v);
        public static SignedNormalized operator -(SignedNormalized a, SignedNormalized b) => new SignedNormalized(a._v - b._v);
        public static SignedNormalized operator *(SignedNormalized a, SignedNormalized b) => new SignedNormalized(a._v * b._v);
        public static SignedNormalized operator /(SignedNormalized a, SignedNormalized b) => new SignedNormalized(a._v / b._v);

        public static bool operator ==(SignedNormalized a, SignedNormalized b) => a._v == b._v;
        public static bool operator !=(SignedNormalized a, SignedNormalized b) => a._v != b._v;
        public static bool operator <(SignedNormalized a, SignedNormalized b) => a._v < b._v;
        public static bool operator >(SignedNormalized a, SignedNormalized b) => a._v > b._v;
        public static bool operator <=(SignedNormalized a, SignedNormalized b) => a._v <= b._v;
        public static bool operator >=(SignedNormalized a, SignedNormalized b) => a._v >= b._v;

        public override string ToString() => _v.ToString("0.##");
        public bool Equals(SignedNormalized other) => _v == other._v;
        public override bool Equals(object obj) => obj is SignedNormalized n && Equals(n);
        public override int GetHashCode() => _v.GetHashCode();
        public int CompareTo(SignedNormalized other) => _v.CompareTo(other._v);
    }
}
