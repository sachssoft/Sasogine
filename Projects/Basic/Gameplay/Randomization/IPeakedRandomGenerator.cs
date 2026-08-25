
namespace Sachssoft.Sasogine.Gameplay.Randomization
{
    public interface IPeakedRandomGenerator : IRandomGenerator
    {
        int Peak { get; init; }
    }
}
