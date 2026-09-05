using System;
using System.Reflection;

namespace Sachssoft.Sasogine;

/// <summary>
/// Provides metadata information about the application assembly.
/// </summary>
public static class GameAppMetadata
{
    private static Assembly Assembly =>
        Assembly.GetEntryAssembly() ??
        Assembly.GetExecutingAssembly();

    /// <summary>
    /// Gets the assembly version.
    /// </summary>
    public static Version AssemblyVersion =>
        Assembly.GetName().Version!;

    /// <summary>
    /// Gets the file version.
    /// </summary>
    public static string FileVersion =>
        Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "";

    /// <summary>
    /// Gets the informational version.
    /// </summary>
    public static string InformationalVersion =>
        Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public static string Product =>
        Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "";

    /// <summary>
    /// Gets the company name.
    /// </summary>
    public static string Company =>
        Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "";

    /// <summary>
    /// Gets the assembly title.
    /// </summary>
    public static string Title =>
        Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "";

    /// <summary>
    /// Gets the assembly description.
    /// </summary>
    public static string Description =>
        Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";

    /// <summary>
    /// Gets the copyright information.
    /// </summary>
    public static string Copyright =>
        Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "";

    /// <summary>
    /// Gets the trademark information.
    /// </summary>
    public static string Trademark =>
        Assembly.GetCustomAttribute<AssemblyTrademarkAttribute>()?.Trademark ?? "";

    /// <summary>
    /// Gets the build configuration.
    /// </summary>
    public static string Configuration =>
        Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? "";
}