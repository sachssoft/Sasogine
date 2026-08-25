//using System;
//using System.DirectoryServices;

//namespace Sachssoft.Sasogine.Gameplay
//{
//    /// <summary>
//    /// Extension methods for TieredScore, including differences, comparisons, clamping, normalization, trends, weighted scoring, and array operations.
//    /// Erweiterungsmethoden für TieredScore: Differenzen, Vergleiche, Clamp, Normalisierung, Trends, Gewichtete Scores, Array-Operationen.
//    /// </summary>
//    public static class TieredScoreExtensions
//    {
//        #region Tier Differences / Differenzen

//        /// <summary>Returns the difference between a value and a specific tier.</summary>
//        // Differenz zu einem bestimmten Tier
//        public static TValue Difference<TValue>(this ITieredScore score, TierResult tier, TValue value)
//            where TValue : struct, IComparable<TValue>
//        {
//            var tierValue = GetTierValue(score, tier);
//            if (typeof(TValue) == typeof(int)) return (TValue)(object)((int)(object)value - (int)tierValue);
//            if (typeof(TValue) == typeof(float)) return (TValue)(object)((float)(object)value - (float)tierValue);
//            if (typeof(TValue) == typeof(TimeSpan)) return (TValue)(object)((TimeSpan)(object)value - (TimeSpan)tierValue);
//            throw new NotSupportedException($"Type {typeof(TValue)} is not supported for Difference.");
//        }

//        /// <summary>Returns the absolute difference to a specific tier.</summary>
//        // Absoluter Unterschied zu einem Tier
//        public static TValue AbsDifference<TValue>(this ITieredScore score, TierResult tier, TValue value)
//            where TValue : struct, IComparable<TValue>
//        {
//            var diff = score.Difference(tier, value);
//            if (typeof(TValue) == typeof(int)) return (TValue)(object)int.Abs((int)(object)diff);
//            if (typeof(TValue) == typeof(float)) return (TValue)(object)float.Abs((float)(object)diff);
//            if (typeof(TValue) == typeof(TimeSpan)) return (TValue)(object)((TimeSpan)(object)diff).Duration();
//            throw new NotSupportedException($"Type {typeof(TValue)} is not supported for AbsDifference.");
//        }

//        /// <summary>Returns the value as percentage of the tier value.</summary>
//        // Prozentwert bezogen auf das Tier
//        public static float PercentOfTier<TValue>(this ITieredScore score, TierResult tier, TValue value)
//            where TValue : struct, IComparable<TValue>
//        {
//            var diff = score.Difference(tier, value);
//            var tierValue = GetTierValue(score, tier);

//            if (typeof(TValue) == typeof(int)) return (int)(object)diff / (float)(int)(object)tierValue;
//            if (typeof(TValue) == typeof(float)) return (float)(object)diff / (float)(object)tierValue;
//            if (typeof(TValue) == typeof(TimeSpan))
//                return (float)((TimeSpan)(object)diff).TotalSeconds / (float)((TimeSpan)(object)tierValue).TotalSeconds;
//            throw new NotSupportedException($"Type {typeof(TValue)} is not supported for PercentOfTier.");
//        }

//        /// <summary>Returns the closest tier to a value.</summary>
//        // Nächstes Tier zu einem Wert
//        public static TierResult ClosestTier<TValue>(this ITieredScore score, TValue value)
//            where TValue : struct, IComparable<TValue>
//        {
//            var bronzeDiff = score.AbsDifference(TierResult.Bronze, value);
//            var silverDiff = score.AbsDifference(TierResult.Silver, value);
//            var goldDiff = score.AbsDifference(TierResult.Gold, value);

//            if (score.LessOrEqual(bronzeDiff, silverDiff) && score.LessOrEqual(bronzeDiff, goldDiff)) return TierResult.Bronze;
//            if (score.LessOrEqual(silverDiff, goldDiff)) return TierResult.Silver;
//            return TierResult.Gold;
//        }

//        private static object GetTierValue(ITieredScore score, TierResult tier)
//        {
//            return tier switch
//            {
//                TierResult.Bronze => score.Bronze,
//                TierResult.Silver => score.Silver,
//                TierResult.Gold => score.Gold,
//                _ => throw new ArgumentException("Invalid tier")
//            };
//        }

//        #endregion

//        #region Comparison / Vergleich

//        public static bool IsBetter<TValue>(this LowTieredScore<TValue> score, TValue a, TValue b)
//            where TValue : struct, IComparable<TValue> => a.CompareTo(b) < 0;

//        public static bool IsBetter<TValue>(this HighTieredScore<TValue> score, TValue a, TValue b)
//            where TValue : struct, IComparable<TValue> => a.CompareTo(b) > 0;

//        public static bool IsBetterOrEqual<TValue>(this LowTieredScore<TValue> score, TValue a, TValue b)
//            where TValue : struct, IComparable<TValue> => a.CompareTo(b) <= 0;

//        public static bool IsBetterOrEqual<TValue>(this HighTieredScore<TValue> score, TValue a, TValue b)
//            where TValue : struct, IComparable<TValue> => a.CompareTo(b) >= 0;

//        public static bool IsWorse<TValue>(this LowTieredScore<TValue> score, TValue a, TValue b)
//            where TValue : struct, IComparable<TValue> => a.CompareTo(b) > 0;

//        public static bool IsWorse<TValue>(this HighTieredScore<TValue> score, TValue a, TValue b)
//            where TValue : struct, IComparable<TValue> => a.CompareTo(b) < 0;

//        public static bool IsWorseOrEqual<TValue>(this LowTieredScore<TValue> score, TValue a, TValue b)
//            where TValue : struct, IComparable<TValue> => a.CompareTo(b) >= 0;

//        public static bool IsWorseOrEqual<TValue>(this HighTieredScore<TValue> score, TValue a, TValue b)
//            where TValue : struct, IComparable<TValue> => a.CompareTo(b) <= 0;

//        public static bool Equals<TValue>(this LowTieredScore<TValue> score, TValue a, TValue b)
//            where TValue : struct, IComparable<TValue> => a.CompareTo(b) == 0;

//        public static bool Equals<TValue>(this HighTieredScore<TValue> score, TValue a, TValue b)
//            where TValue : struct, IComparable<TValue> => a.CompareTo(b) == 0;

//        #endregion

//        #region Clamp / Normalize / Trend

//        public static TValue ClampToTiers<TValue>(this TieredScore<TValue> score, TValue value)
//            where TValue : struct, IComparable<TValue>
//        {
//            var min = score.Direction == ScoreDirection.Low ? score.Bronze : score.Gold;
//            var max = score.Direction == ScoreDirection.Low ? score.Gold : score.Bronze;
//            return TieredScore<TValue>.Clamp(value, min, max);
//        }

//        public static float NormalizedScore<TValue>(this TieredScore<TValue> score, TValue value)
//            where TValue : struct, IComparable<TValue>
//        {
//            var min = score.Direction == ScoreDirection.Low ? score.Bronze : score.Gold;
//            var max = score.Direction == ScoreDirection.Low ? score.Gold : score.Bronze;

//            if (typeof(TValue) == typeof(int))
//            {
//                int v = (int)(object)value, mi = (int)(object)min, ma = (int)(object)max;
//                return (float)(v - mi) / (ma - mi);
//            }
//            if (typeof(TValue) == typeof(float))
//            {
//                float v = (float)(object)value, mi = (float)(object)min, ma = (float)(object)max;
//                return (v - mi) / (ma - mi);
//            }
//            if (typeof(TValue) == typeof(TimeSpan))
//            {
//                double v = ((TimeSpan)(object)value).TotalSeconds;
//                double mi = ((TimeSpan)(object)min).TotalSeconds;
//                double ma = ((TimeSpan)(object)max).TotalSeconds;
//                return (float)((v - mi) / (ma - mi));
//            }
//            throw new NotSupportedException();
//        }

//        public static int Trend<TValue>(this TieredScore<TValue> score, TValue previous, TValue current)
//            where TValue : struct, IComparable<TValue>
//        {
//            if (score.IsBetter(current, previous)) return 1;
//            if (score.IsWorse(current, previous)) return -1;
//            return 0;
//        }

//        public static int Trend<TValue>(this TieredScore<TValue> score, TValue previous, TValue current)
//            where TValue : struct, IComparable<TValue>
//        {
//            if (score.IsBetter(current, previous)) return 1;
//            if (score.IsWorse(current, previous)) return -1;
//            return 0;
//        }

//        #endregion

//        #region Weighted / Combine Scores

//        /// <summary>Returns a weighted score depending on tier.</summary>
//        // Gewichteter Score nach Tier
//        public static float WeightedScore<TValue>(this TieredScore<TValue> score, TValue value, float bronzeWeight = 1f, float silverWeight = 2f, float goldWeight = 3f)
//            where TValue : struct, IComparable<TValue>
//        {
//            var tier = score.GetResult(value);
//            return tier switch
//            {
//                TierResult.Bronze => bronzeWeight,
//                TierResult.Silver => silverWeight,
//                TierResult.Gold => goldWeight,
//                _ => 0f
//            };
//        }

//        /// <summary>Combines multiple weighted scores into one average value.</summary>
//        // Durchschnitt von gewichteten Scores
//        public static float CombineScores(params float[] weightedScores)
//        {
//            if (weightedScores == null || weightedScores.Length == 0) return 0f;
//            float total = 0f;
//            foreach (var w in weightedScores) total += w;
//            return total / weightedScores.Length;
//        }

//        #endregion

//        #region Array / Batch Operations

//        /// <summary>Returns weighted scores for an array of values.</summary>
//        // Gewichtete Scores für ein Array von Werten
//        public static float[] WeightedScores<TValue>(this TieredScore<TValue> score, TValue[] values, float bronzeWeight = 1f, float silverWeight = 2f, float goldWeight = 3f)
//            where TValue : struct, IComparable<TValue>
//        {
//            if (values == null) throw new ArgumentNullException(nameof(values));
//            var result = new float[values.Length];
//            for (int i = 0; i < values.Length; i++)
//                result[i] = score.WeightedScore(values[i], bronzeWeight, silverWeight, goldWeight);
//            return result;
//        }

//        /// <summary>Returns normalized scores (0..1) for an array of values.</summary>
//        // Normalisierte Scores für ein Array von Werten
//        public static float[] NormalizedScores<TValue>(this TieredScore<TValue> score, TValue[] values)
//            where TValue : struct, IComparable<TValue>
//        {
//            if (values == null) throw new ArgumentNullException(nameof(values));
//            var result = new float[values.Length];
//            for (int i = 0; i < values.Length; i++)
//                result[i] = score.NormalizedScore(values[i]);
//            return result;
//        }

//        /// <summary>Returns the closest tier for each value in an array.</summary>
//        // Nächstes Tier für jedes Element im Array
//        public static TierResult[] ClosestTiers<TValue>(this TieredScore<TValue> score, TValue[] values)
//            where TValue : struct, IComparable<TValue>
//        {
//            if (values == null) throw new ArgumentNullException(nameof(values));
//            var result = new TierResult[values.Length];
//            for (int i = 0; i < values.Length; i++)
//                result[i] = score.ClosestTier(values[i]);
//            return result;
//        }

//        #endregion
//    }
//}
