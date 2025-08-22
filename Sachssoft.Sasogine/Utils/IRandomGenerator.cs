namespace Sachssoft.Sasogine.Utils;

public interface IRandomGenerator
{

    int Minimum { get; init; }

    int Maximum { get; init; }

    int Seed { get; init; }

    int Generate();

}

public interface IPeakedRandomGenerator : IRandomGenerator
{
    int Peak { get; init; }
}