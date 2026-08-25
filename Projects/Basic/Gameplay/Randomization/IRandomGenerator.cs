
namespace Sachssoft.Sasogine.Gameplay.Randomization
{
    public interface IRandomGenerator
    {

        int Minimum { get; init; }

        int Maximum { get; init; }

        int Seed { get; init; }

        int Generate();

    }
}