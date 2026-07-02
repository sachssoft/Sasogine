using System;

namespace Sachssoft.Sasogine.Gameplay
{
    public readonly struct LowTieredScore<TValue> : ITieredScore where TValue : struct, IComparable<TValue>
    {
        public readonly TValue Bronze;
        public readonly TValue Silver;
        public readonly TValue Gold;

        public LowTieredScore(TValue bronze, TValue silver, TValue gold)
        {
            // Clamp: Bronze <= Silver <= Gold
            Bronze = Min(bronze, Min(silver, gold));
            Silver = Clamp(silver, Bronze, Max(silver, gold));
            Gold = Max(gold, Max(bronze, silver));
        }

        internal static TValue Min(TValue a, TValue b) => a.CompareTo(b) <= 0 ? a : b;
        internal static TValue Max(TValue a, TValue b) => a.CompareTo(b) >= 0 ? a : b;
        internal static TValue Clamp(TValue value, TValue min, TValue max)
        {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }

        public LowTieredScore<TValue> ChangeGold(TValue gold) => new LowTieredScore<TValue>(Bronze, Silver, gold);
        public LowTieredScore<TValue> ChangeSilver(TValue silver) => new LowTieredScore<TValue>(Bronze, silver, Gold);
        public LowTieredScore<TValue> ChangeBronze(TValue bronze) => new LowTieredScore<TValue>(bronze, Silver, Gold);

        public HighTieredScore<TValue> ToHigh()
        {
            return new HighTieredScore<TValue>(Bronze, Silver, Gold);
        }

        public TierResult GetResult(TValue value)
        {
            return value.CompareTo(Gold) <= 0 ? TierResult.Gold :
                        value.CompareTo(Silver) <= 0 ? TierResult.Silver :
                        value.CompareTo(Bronze) <= 0 ? TierResult.Bronze :
                        TierResult.None;
        }
        public static bool operator ==(LowTieredScore<TValue> a, LowTieredScore<TValue> b)
        {
            return a.Bronze.CompareTo(b.Bronze) == 0 &&
                   a.Silver.CompareTo(b.Silver) == 0 &&
                   a.Gold.CompareTo(b.Gold) == 0;
        }

        public static bool operator !=(LowTieredScore<TValue> a, LowTieredScore<TValue> b) => !(a == b);

        // Vergleicht zwei Werte gemäß ScoreDirection
        public static bool operator <(LowTieredScore<TValue> score, TValue value)
        {
            return value.CompareTo(score.Bronze) < 0;
        }

        public static bool operator >(LowTieredScore<TValue> score, TValue value)
        {
            return value.CompareTo(score.Gold) > 0;
        }

        public static bool operator <=(LowTieredScore<TValue> score, TValue value)
        {
            return value.CompareTo(score.Gold) <= 0;
        }

        public static bool operator >=(LowTieredScore<TValue> score, TValue value)
        {
            return value.CompareTo(score.Bronze) >= 0;
        }

        // Für structs auch Equals/GetHashCode implementieren
        public override bool Equals(object? obj)
        {
            return obj is LowTieredScore<TValue> other && this == other;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Bronze, Silver, Gold);
        }

        // ----------------------
        // ITieredScore Implementation
        // ----------------------
        Type ITieredScore.Type => typeof(TValue);

        object ITieredScore.Bronze => Bronze!;

        object ITieredScore.Silver => Silver!;

        object ITieredScore.Gold => Gold!;

        ITieredScore ITieredScore.WithValues(object bronze, object silver, object gold)
        {
            if (bronze is not TValue b) throw new ArgumentException($"bronze must be of type {typeof(TValue).Name}");
            if (silver is not TValue s) throw new ArgumentException($"silver must be of type {typeof(TValue).Name}");
            if (gold is not TValue g) throw new ArgumentException($"gold must be of type {typeof(TValue).Name}");

            return new LowTieredScore<TValue>(b, s, g);
        }

        TierResult ITieredScore.GetResult(object value)
        {
            if (value is not TValue typedValue)
                throw new ArgumentException($"Value must be of type {typeof(TValue).Name}", nameof(value));

            return GetResult(typedValue);
        }
    }
}
