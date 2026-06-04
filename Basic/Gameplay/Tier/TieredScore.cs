using System;
using System.Numerics;

namespace Sachssoft.Sasogine.Gameplay
{
    public readonly struct TieredScore<TValue> : ITieredScore where TValue : struct, IComparable<TValue>
    {
        public readonly TValue Bronze;
        public readonly TValue Silver;
        public readonly TValue Gold;
        public readonly ScoreDirection Direction;

        public TieredScore(TValue bronze, TValue silver, TValue gold, ScoreDirection direction)
        {
            Direction = direction;

            if (direction == ScoreDirection.Low)
            {
                // Clamp: Bronze <= Silver <= Gold
                Bronze = Min(bronze, Min(silver, gold));
                Silver = Clamp(silver, Bronze, Max(silver, gold));
                Gold = Max(gold, Max(bronze, silver));
            }
            else
            {
                // High: Bronze >= Silver >= Gold
                Bronze = Max(bronze, Max(silver, gold));
                Silver = Clamp(silver, Min(silver, gold), Bronze);
                Gold = Min(gold, Min(bronze, silver));
            }
        }

        internal static TValue Min(TValue a, TValue b) => a.CompareTo(b) <= 0 ? a : b;
        internal static TValue Max(TValue a, TValue b) => a.CompareTo(b) >= 0 ? a : b;
        internal static TValue Clamp(TValue value, TValue min, TValue max)
        {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }

        public TieredScore<TValue> ChangeGold(TValue gold) => new TieredScore<TValue>(Bronze, Silver, gold, Direction);
        public TieredScore<TValue> ChangeSilver(TValue silver) => new TieredScore<TValue>(Bronze, silver, Gold, Direction);
        public TieredScore<TValue> ChangeBronze(TValue bronze) => new TieredScore<TValue>(bronze, Silver, Gold, Direction);

        public TierResult GetResult(TValue value)
        {
            return Direction switch
            {
                ScoreDirection.Low => value.CompareTo(Gold) <= 0 ? TierResult.Gold :
                                      value.CompareTo(Silver) <= 0 ? TierResult.Silver :
                                      value.CompareTo(Bronze) <= 0 ? TierResult.Bronze :
                                      TierResult.None,

                ScoreDirection.High => value.CompareTo(Gold) >= 0 ? TierResult.Gold :
                                       value.CompareTo(Silver) >= 0 ? TierResult.Silver :
                                       value.CompareTo(Bronze) >= 0 ? TierResult.Bronze :
                                       TierResult.None,

                _ => TierResult.None
            };
        }
        public static bool operator ==(TieredScore<TValue> a, TieredScore<TValue> b)
        {
            return a.Bronze.CompareTo(b.Bronze) == 0 &&
                   a.Silver.CompareTo(b.Silver) == 0 &&
                   a.Gold.CompareTo(b.Gold) == 0 &&
                   a.Direction == b.Direction;
        }

        public static bool operator !=(TieredScore<TValue> a, TieredScore<TValue> b) => !(a == b);

        // Vergleicht zwei Werte gemäß ScoreDirection
        public static bool operator <(TieredScore<TValue> score, TValue value)
        {
            return score.Direction == ScoreDirection.Low
                ? value.CompareTo(score.Bronze) < 0
                : value.CompareTo(score.Bronze) > 0;
        }

        public static bool operator >(TieredScore<TValue> score, TValue value)
        {
            return score.Direction == ScoreDirection.Low
                ? value.CompareTo(score.Gold) > 0
                : value.CompareTo(score.Gold) < 0;
        }

        public static bool operator <=(TieredScore<TValue> score, TValue value)
        {
            return score.Direction == ScoreDirection.Low
                ? value.CompareTo(score.Gold) <= 0
                : value.CompareTo(score.Gold) >= 0;
        }

        public static bool operator >=(TieredScore<TValue> score, TValue value)
        {
            return score.Direction == ScoreDirection.Low
                ? value.CompareTo(score.Bronze) >= 0
                : value.CompareTo(score.Bronze) <= 0;
        }

        // Für structs auch Equals/GetHashCode implementieren
        public override bool Equals(object? obj)
        {
            return obj is TieredScore<TValue> other && this == other;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Bronze, Silver, Gold, Direction);
        }

        // ----------------------
        // ITieredScore Implementation
        // ----------------------
        Type ITieredScore.Type => typeof(TValue);

        object ITieredScore.Bronze => Bronze!;

        object ITieredScore.Silver => Silver!;

        object ITieredScore.Gold => Gold!;

        ScoreDirection ITieredScore.Direction => Direction;

        ITieredScore ITieredScore.WithValues(object bronze, object silver, object gold)
        {
            if (bronze is not TValue b) throw new ArgumentException($"bronze must be of type {typeof(TValue).Name}");
            if (silver is not TValue s) throw new ArgumentException($"silver must be of type {typeof(TValue).Name}");
            if (gold is not TValue g) throw new ArgumentException($"gold must be of type {typeof(TValue).Name}");

            return new TieredScore<TValue>(b, s, g, Direction);
        }

        TierResult ITieredScore.GetResult(object value)
        {
            if (value is not TValue typedValue)
                throw new ArgumentException($"Value must be of type {typeof(TValue).Name}", nameof(value));

            return GetResult(typedValue);
        }
    }
}
