namespace Sachssoft.Sasogine.Containers
{
    public interface IPackageLevelFactory
    {
        PackageLevelBase Build(PackageBase package, string filePath);
    }
}