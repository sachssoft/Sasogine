using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Gameplay.Randomization
{
    public class SineRandomGenerator : IPeakedRandomGenerator
    {
        public int Minimum { get; init; }
        public int Maximum { get; init; }
        public int Seed { get; init; }
        public int Peak { get; init; }

        public int Generate()
        {
            if (Minimum >= Maximum)
                throw new ArgumentException("Minimum must be less than maximum.");

            int range = Maximum - Minimum;
            int count = range + 1;

            var random = new Random(Seed);

            // Erzeuge Gewichtungen basierend auf einer Sinus-Kurve
            List<(int value, float weight)> weighted = new();

            for (int i = 0; i < count; i++)
            {
                int value = Minimum + i;

                // Verschiebung der Sinus-Kurve zum gewünschten Peak
                float shifted_x = (float)(value - Peak) / range * float.Pi;
                float weight = float.Cos(shifted_x); // Peak = max bei cos(0)

                if (weight < 0)
                    weight = 0;

                weighted.Add((value, weight));
            }

            // Normalisiere die Gewichte
            float total_weight = weighted.Sum(w => w.weight);
            float rand = (float)random.NextDouble() * total_weight;

            // Wähle basierend auf Gewichtung
            foreach (var (value, weight) in weighted)
            {
                if (rand < weight)
                    return value;
                rand -= weight;
            }

            return Maximum; // Fallback
        }
    }
}