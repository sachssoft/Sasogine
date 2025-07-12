using Microsoft.Xna.Framework;
using AssetManagementBase;
using tcs.Game.UI.Visuals.Controls;

namespace tcs.Game.UI.Samples;

public class AssetManagementGame : Microsoft.Xna.Framework.Game
{
	private readonly GraphicsDeviceManager _graphics;
	private MainForm _mainForm;
	private Desktop _desktop;

	public AssetManagementGame()
	{
		_graphics = new GraphicsDeviceManager(this)
		{
			PreferredBackBufferWidth = 1200,
			PreferredBackBufferHeight = 800
		};
		Window.AllowUserResizing = true;
		IsMouseVisible = true;
	}

	protected override void LoadContent()
	{
		base.LoadContent();

		UIEnvironment.Game = this;

        UIEnvironment.DefaultAssetManager = AssetManager.CreateFileAssetManager(Path.Combine(PathUtils.ExecutingAssemblyDirectory, "Assets"));

		_mainForm = new MainForm();
		_mainForm._mainMenu.HoverIndex = 0;
		_mainForm._menuItemQuit.Selected += (s, a) => Exit();

		_desktop = new Desktop
		{
			FocusedKeyboardWidget = _mainForm._mainMenu
		};

		// Make main menu permanently hold keyboard focus
		_desktop.WidgetLosingKeyboardFocus += (s, a) =>
		{
			a.Cancel = true;
		};

		_desktop.Root = _mainForm;

        // Inform Myra that external text input is available
        // So it stops translating Keys to chars
        _desktop.HasExternalTextInput = true;

		// Provide that text input
		Window.TextInput += (s, a) =>
		{
            _desktop.OnChar(a.Character);
		};
	}

	protected override void Draw(GameTime gameTime)
	{
		base.Draw(gameTime);

		GraphicsDevice.Clear(Color.Black);
		_desktop.Render();
	}
}