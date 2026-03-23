using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.Utilities;
using Sachssoft.Sasogine.Platform;
using Sachssoft.Sasogine.Surface.Controls;
using Sachssoft.Sasogine.Surface.Interactions;
using Sachssoft.Sasogine.Surface.Services;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

#if FNA
using static SDL2.SDL;
using MouseCursor = System.Nullable<System.IntPtr>;
#endif

namespace Sachssoft.Sasogine.Surface.Basic;

public static class UIEnvironment
{
    #region Font 

    //private static readonly Dictionary<string, FontTypeface> _fonts = new(StringComparer.OrdinalIgnoreCase);
    ///// <summary>
    ///// Registers a new font with its faces (weight + style variants).
    ///// </summary>
    ///// <param name="name">The font family name</param>
    ///// <param name="faces">The font faces to register</param>
    //public static void RegisterFont(string name, FontVariant[] faces)
    //{
    //    if (string.IsNullOrWhiteSpace(name))
    //        throw new ArgumentNullException(nameof(name));

    //    if (faces == null || faces.Length == 0)
    //        throw new ArgumentNullException(nameof(faces));

    //    if (!_fonts.TryGetValue(name, out var font))
    //    {
    //        font = new FontTypeface(name, faces);
    //        _fonts[name] = font;
    //    }
    //}

    //public static FontTypeface DefaultFontFamily =>
    //    throw new NotImplementedException();

    ///// <summary>
    ///// Retrieves a font by name.
    ///// </summary>
    ///// <param name="name">The font family name</param>
    ///// <returns>The Font instance, or null if not found</returns>
    //public static FontTypeface? GetFontFamily(string name)
    //{
    //    _fonts.TryGetValue(name, out var font);
    //    return font;
    //}

    ///// <summary>
    ///// Retrieves a specific FontFace by font name, weight, and style.
    ///// </summary>
    //public static FontVariant? GetFontFace(string name, FontWeight weight = FontWeight.Regular, FontStyle style = FontStyle.Normal)
    //{
    //    var font = GetFontFamily(name);
    //    return font?.GetFace(weight, style);
    //}

    #endregion

#if MONOGAME
    private static readonly Dictionary<MouseCursorType, MouseCursor> _mouseCursors = new Dictionary<MouseCursorType, MouseCursor>
    {
        [MouseCursorType.Arrow] = MouseCursor.Arrow,
        [MouseCursorType.IBeam] = MouseCursor.IBeam,
        [MouseCursorType.Wait] = MouseCursor.Wait,
        [MouseCursorType.Crosshair] = MouseCursor.Crosshair,
        [MouseCursorType.WaitArrow] = MouseCursor.WaitArrow,
        [MouseCursorType.SizeNWSE] = MouseCursor.SizeNWSE,
        [MouseCursorType.SizeNESW] = MouseCursor.SizeNESW,
        [MouseCursorType.SizeWE] = MouseCursor.SizeWE,
        [MouseCursorType.SizeNS] = MouseCursor.SizeNS,
        [MouseCursorType.SizeAll] = MouseCursor.SizeAll,
        [MouseCursorType.No] = MouseCursor.No,
        [MouseCursorType.Hand] = MouseCursor.Hand,
    };
#elif FNA
	private static readonly Dictionary<SDL_SystemCursor, IntPtr> _systemCursors = new Dictionary<SDL_SystemCursor, IntPtr>();

	private static IntPtr GetSystemCursor(SDL_SystemCursor type)
	{
		IntPtr result;
		if (_systemCursors.TryGetValue(type, out result))
		{
			return result;
		}

		result = SDL_CreateSystemCursor(type);
		_systemCursors[type] = result;

		return result;
	}

	private static readonly Dictionary<MouseCursorType, SDL_SystemCursor> _mouseCursors = new Dictionary<MouseCursorType, SDL_SystemCursor>
	{
		[MouseCursorType.Arrow] = SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW,
		[MouseCursorType.IBeam] = SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM,
		[MouseCursorType.Wait] = SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAIT,
		[MouseCursorType.Crosshair] = SDL_SystemCursor.SDL_SYSTEM_CURSOR_CROSSHAIR,
		[MouseCursorType.WaitArrow] = SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAITARROW,
		[MouseCursorType.SizeNWSE] = SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENWSE,
		[MouseCursorType.SizeNESW] = SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENESW,
		[MouseCursorType.SizeWE] = SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE,
		[MouseCursorType.SizeNS] = SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENS,
		[MouseCursorType.SizeAll] = SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL,
		[MouseCursorType.No] = SDL_SystemCursor.SDL_SYSTEM_CURSOR_NO,
		[MouseCursorType.Hand] = SDL_SystemCursor.SDL_SYSTEM_CURSOR_HAND,
	};
#endif

    private static MouseCursorType _mouseCursorType;
    //private static AssetManager _defaultAssetManager;

    public static bool SetMouseCursorFromWidget { get; set; } = true;

    public static MouseCursorType MouseCursorType
    {
        get => _mouseCursorType;
        set
        {
            if (_mouseCursorType == value)
            {
                return;
            }

            _mouseCursorType = value;

#if MONOGAME
            MouseCursor mouseCursor;
            if (!_mouseCursors.TryGetValue(value, out mouseCursor))
            {
                throw new Exception($"Could not find mouse cursor {value}");
            }

            Mouse.SetCursor(mouseCursor);
#elif FNA
			SDL_SystemCursor mouseCursor;
			if (!_mouseCursors.TryGetValue(value, out mouseCursor))
			{
				throw new Exception($"Could not find mouse cursor {value}");
			}

			var mouseCursorPtr = GetSystemCursor(mouseCursor);
			SDL2.SDL.SDL_SetCursor(mouseCursorPtr);
#endif
        }
    }

    public static MouseCursorType DefaultMouseCursorType { get; set; }

    private static Game _game;

    public static Game Game
    {
        get
        {
            if (_game == null)
            {
                _game = IGameApplication.Game;
                //_game.Window.TextInput += Window_TextInput;
                //_game.Window.KeyUp += Window_KeyUp;
                //throw new Exception("UIEnvironment.Sasogine is null. Please, set it to the Sasogine instance before using GameUI.");
            }

            return _game;
        }

        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (_game == value)
            {
                return;
            }

            if (_game != null)
            {
                //_game.Window.TextInput -= Window_TextInput;
                //_game.Window.KeyUp -= Window_KeyUp;
                _game.Disposed -= GameOnDisposed;
            }

            _game = value;
            if (_game != null)
            {
                //_game.Window.TextInput += Window_TextInput;
                //_game.Window.KeyUp += Window_KeyUp;
                _game.Disposed += GameOnDisposed;
            }
        }
    }

    public static CultureInfo Culture
    {
        get; set;
    } = CultureInfo.InvariantCulture;

    //private static void Window_KeyUp(object? sender, InputKeyEventArgs e)
    //{
    //    _textInputState = new TextInputState();
    //}

    //private static void Window_TextInput(object? sender, TextInputEventArgs e)
    //{
    //    _textInputState = new TextInputState()
    //    {
    //        Character = e.Character,
    //        Key = e.Key
    //    };
    //}

    public static IClipboardService? Clipboard => _clipboard;

    public static TextInputState TextInputState => _textInputState;

    public static GraphicsDevice GraphicsDevice
    {
        get => Game.GraphicsDevice;
    }

    /// <summary>
    /// Default Assets Manager
    /// </summary>
    //public static AssetManager DefaultAssetManager
    //{
    //    get
    //    {
    //        if (_defaultAssetManager == null)
    //        {
    //            _defaultAssetManager = AssetManager.CreateFileAssetManager(PathUtils.ExecutingAssemblyDirectory);
    //        }

    //        return _defaultAssetManager;
    //    }

    //    set
    //    {
    //        if (value == null)
    //        {
    //            throw new ArgumentNullException(nameof(value));

    //        }
    //        _defaultAssetManager = value;
    //    }
    //}

    public static bool DrawWidgetsFrames { get; set; }
    public static bool DrawKeyboardFocusedWidgetFrame { get; set; }
    public static bool DrawMouseHoveredWidgetFrame { get; set; }
    public static bool DrawTextGlyphsFrames { get; set; }
    public static bool DisableClipping { get; set; }

    public static Func<MouseInfo> MouseInfoGetter { get; set; } = DefaultMouseInfoGetter;
    public static Action<bool[]> DownKeysGetter { get; set; } = DefaultDownKeysGetter;

    public static int DoubleClickIntervalInMs { get; set; } = 500;
    public static int DoubleClickRadius { get; set; } = 2;
    public static int TooltipDelayInMs { get; set; } = 500;
    public static Point TooltipOffset { get; set; } = new Point(0, 20);
    public static Func<Widget, Widget> TooltipCreator { get; set; } = w =>
    {
        var tooltip = new Label()
        {
            //Text = w.Tooltip,
            Tag = w
        };

        //tooltip.ApplyLabelStyle(Stylesheet.Current.TooltipStyle);

        return tooltip;
    };

    /// <summary>
    /// Makes the text rendering more smooth(especially when scaling) for the cost of sacrificing some performance 
    /// </summary>
    public static bool SmoothText { get; set; }
    public static bool EnableModalDarkening { get; set; }

    public static Color DarkeningColor { get; set; } = new Color(0, 0, 0, 192);

    private static void GameOnDisposed(object? sender, EventArgs eventArgs)
    {
        Reset();
    }

    /// <summary>
    /// 
    /// </summary>
    public static void Reset()
    {
        //DefaultAssets.Dispose();
        //Stylesheet.Current = null;
    }

    public static string Version
    {
        get
        {
            var assembly = typeof(UIEnvironment).Assembly;
            var name = new AssemblyName(assembly.FullName);

            return name.Version.ToString();
        }
    }

    internal static string InternalClipboard;
    private static TextInputState _textInputState;
    private static IClipboardService _clipboard;

    public static MouseInfo DefaultMouseInfoGetter()
    {
        var state = Mouse.GetState();

        return new MouseInfo
        {
            Position = new Point(state.X, state.Y),
            IsLeftButtonDown = Game.IsActive && state.LeftButton == ButtonState.Pressed,
            IsMiddleButtonDown = Game.IsActive && state.MiddleButton == ButtonState.Pressed,
            IsRightButtonDown = Game.IsActive && state.RightButton == ButtonState.Pressed,
            Wheel = state.ScrollWheelValue
        };
    }

    public static void DefaultDownKeysGetter(bool[] keys)
    {
        var state = Keyboard.GetState();
        for (var i = 0; i < keys.Length; ++i)
        {
            keys[i] = state.IsKeyDown((Keys)i);
        }
    }

    public static bool IsMobile
    {
        get
        {
#if MONOGAME
            return PlatformInfo.MonoGamePlatform == MonoGamePlatform.Android ||
                PlatformInfo.MonoGamePlatform == MonoGamePlatform.iOS;
#else
			return false;
#endif
        }
    }
}