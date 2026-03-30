using Sachssoft.Sasogine.Surface.Controls;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Styles
{
    // AOT-Freundliche Registrierung von Styleable-Widget-Typen
    public static class StyleableTypeRegistry
    {
        private static readonly Dictionary<string, (Type Type, Func<Widget> Factory)> _types =
            new(StringComparer.OrdinalIgnoreCase);

        static StyleableTypeRegistry()
        {
            RegisterStyleableTypes();
        }

        /// <summary>
        /// Registriert einen Widget-Typ mit einem String-Key.
        /// </summary>
        public static void Register<T>(string key) where T : Widget, new()
        {
            if (_types.ContainsKey(key))
                throw new InvalidOperationException($"A factory for key '{key}' is already registered.");

            _types[key] = (typeof(T), () => new T());
        }

        /// <summary>
        /// Gibt den registrierten Typ für den Key zurück (oder null, falls nicht gefunden)
        /// </summary>
        public static Type? FindType(string? key)
        {
            return _types.TryGetValue(key ?? string.Empty, out var entry) ? entry.Type : null;
        }

        /// <summary>
        /// Erstellt eine neue Instanz des registrierten Widgets für den Key
        /// </summary>
        public static Widget CreateInstance(string key)
        {
            if (_types.TryGetValue(key, out var entry))
                return entry.Factory();

            throw new InvalidOperationException($"No factory registered for styleable type '{key}'.");
        }

        private static void RegisterStyleableTypes()
        {
            Register<Button>("Button");
            Register<CheckBox>("CheckBox");
            Register<ColorPickerDialog>("ColorPickerDialog");
            Register<ChoiceButton>("ChoiceButton");
            Register<ComboBox>("ComboBox");
            Register<ContentControl>("ContentControl");
            Register<DropdownButton>("DropdownButton");
            Register<FileSystemDialog>("FileDialog");
            Register<Foldout>("Foldout");
            Register<HorizontalSplitPane>("HorizontalSplitPane");
            Register<Image>("Image");
            Register<Label>("Label");
            Register<ListView>("ListView");
            Register<Menu>("Menu");
            Register<MenuItemPresenter>("MenuItemPresenter");
            Register<MenuItemView>("MenuItemView");
            Register<NumberSpinner>("NumberSpinner");
            Register<ProgressBar>("ProgressBar");
            Register<RadioButton>("RadioButton");
            Register<RepeatButton>("RepeatButton");
            Register<ScrollViewer>("ScrollViewer");
            Register<SelectableItemPresenter>("SelectableItemPresenter");
            Register<Separator>("Separator");
            Register<Slider>("Slider");
            Register<Switch>("Switch");
            Register<TabControl>("TabControl");
            Register<TextBox>("TextBox");
            Register<ToggleButton>("ToggleButton");
            Register<Toolbar>("Toolbar");
            //Register<TreeView>("Tree");
            Register<VerticalSplitPane>("VerticalSplitPane");
            Register<Window>("Window");
        }
    }
}
