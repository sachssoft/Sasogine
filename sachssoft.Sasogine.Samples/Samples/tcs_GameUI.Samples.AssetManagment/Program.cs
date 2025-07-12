using System;

namespace tcs.Game.UI.Samples;

class Program
{
	static void Main(string[] args)
	{
		try
		{
			using (var game = new AssetManagementGame())
				game.Run();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}
}
