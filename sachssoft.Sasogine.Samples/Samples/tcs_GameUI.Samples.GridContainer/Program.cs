using tcs.Game.UI.Samples;

namespace tcs.Game.UI.Samples;

internal class Program
{
	public static void Main(string[] args)
	{
		using (var game = new GridGame())
			game.Run();
	}
}