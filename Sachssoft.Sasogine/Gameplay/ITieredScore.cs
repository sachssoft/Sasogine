using System;

namespace Sachssoft.Sasogine.Gameplay
{
    public interface ITieredScore
    {
        Type Type { get; }

        object Bronze { get; }

        object Silver { get; }

        object Gold { get; }

        ITieredScore WithValues(object bronze, object silver, object gold);

        ScoreDirection Direction { get; }

        TierResult GetResult(object value);

    }
}
