using System;

namespace Sachssoft.Sasogine.Gameplay
{
    public static class TieredScoreFactory
    {
        public static TieredScore<TimeSpan> CreateTimeScore(TimeSpan bronze, TimeSpan silver, TimeSpan gold)
            => new TieredScore<TimeSpan>(bronze, silver, gold, ScoreDirection.Low);

        public static TieredScore<int> CreateIntScore(int bronze, int silver, int gold, ScoreDirection direction)
            => new TieredScore<int>(bronze, silver, gold, direction);

        public static TieredScore<float> CreateFloatScore(float bronze, float silver, float gold, ScoreDirection direction)
            => new TieredScore<float>(bronze, silver, gold, direction);
    }

}
