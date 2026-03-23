using Sachssoft.Sasofly.Documents.Naming;

namespace Sachssoft.Sasogine.Containers
{
    /// <summary>
    /// Factory interface for creating package levels from files.
    /// </summary>
    public interface IPackageLevelFactory
    {
        /// <summary>
        /// Builds a <see cref="PackageLevelBase"/> instance from the specified package and file path.
        /// </summary>
        /// <param name="package">The package that contains the level.</param>
        /// <param name="filePath">The file path to the level data.</param>
        /// <returns>A new instance of <see cref="PackageLevelBase"/> representing the level.</returns>
        PackageLevelBase Build(PackageBase package, string filePath);


        INamingConvention? NamingConvention { get; }
    }
}
