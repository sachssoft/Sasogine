using System;
using System.Reflection;

namespace sachssoft.Sasogine;

public static class GameAppMetadata
{
    private static Assembly Assembly => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

    public static Version AssemblyVersion => Assembly.GetName().Version!;
    public static string FileVersion => Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "";
    public static string InformationalVersion => Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";
    public static string Product => Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "";
    public static string Company => Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "";
    public static string Title => Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "";
    public static string Description => Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";
    public static string Copyright => Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "";
    public static string Trademark => Assembly.GetCustomAttribute<AssemblyTrademarkAttribute>()?.Trademark ?? "";
    public static string Configuration => Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? "";

}
