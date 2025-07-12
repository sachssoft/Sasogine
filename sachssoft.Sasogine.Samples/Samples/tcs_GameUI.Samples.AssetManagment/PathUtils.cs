using System.IO;
using System.Reflection;
using System;

namespace tcs.Game.UI.Samples;

internal static class PathUtils
{
	public static string ExecutingAssemblyDirectory
	{
		get
		{
			string codeBase = Assembly.GetExecutingAssembly().Location;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			return Path.GetDirectoryName(path);
		}
	}
}
