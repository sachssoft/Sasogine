#if IOS
using UIKit;

namespace SASOGINE_ROOT_NAMESPACE;

/// <summary>iOS program entry point.</summary>
public static class Program
{
    /// <summary>Starts the native iOS application.</summary>
    /// <param name="args">Process arguments.</param>
    public static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
#endif
