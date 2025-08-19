using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using Sachssoft.Sasogine.Surface.Design;

namespace Sachssoft.Sasogine.Surface.Forms;

public class PropertyListBuilder : IContentBuilder<ContentControl>
{
    private static List<PropertyEditorBase>? _editor_types;
    private List<PropertyEditorBase>? _custom_editor_types;
    private object? _source;

    public PropertyListBuilder(object? source)
    {
        _source = source;
    }

    private static PropertyEditorBase[] GetEditorTypes()
    {
        var asm = typeof(PropertyListBuilder).Assembly;
        return asm.GetTypes()
                  .Where(x => x.IsAssignableTo(typeof(PropertyEditorBase)) && !x.IsAbstract)
                  .Select(x => (PropertyEditorBase)Activator.CreateInstance(x)!)
                  .ToArray();
    }

    public void AddCustomEditor<T>() where T : PropertyEditorBase, new()
    {
        if (_custom_editor_types == null)
            _custom_editor_types = new();

        _custom_editor_types.Add(new T());
    }

    public Desktop Desktop
    {
        get;
        set;
    }

    public Action<PropertyEditorBase>? Browse
    {
        get;
        set;
    }

    public Action<PropertyEditorBase>? Initialize
    {
        get;
        set;
    }

    public Action<PropertyEditorBase>? Changed
    {
        get;
        set;
    }

    public Func<string[], string[]>? CategoryFilterCallback
    {
        get;
        set;
    }

    public void Build(ContentControl owner)
    {
        throw new NotImplementedException("");

        //if (_source == null)
        //{
        //    owner.Content = null;
        //    return;
        //}

        //if (Desktop == null)
        //    throw new NotImplementedException("Desktop required");

        //var pnl = new VerticalStackPanel()
        //{
        //    Spacing = 10
        //};

        //var type = _source.GetType();
        //var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //var text_info = CultureInfo.InvariantCulture.TextInfo;

        //var sorted_properties = new Dictionary<string, List<PropertyInfo>>();
        //sorted_properties.Add("", new()); // Erste Stelle als Misc

        //// Sortiere jetzt nach Kategorien
        //foreach (var property in properties)
        //{
        //    if (property.SetMethod == null || !property.SetMethod.IsPublic)
        //        continue;

        //    var eb_attr = property.GetCustomAttribute<BrowsableAttribute>();
        //    var c_attr = property.GetCustomAttribute<FieldCategoryAttribute>();

        //    // Sollte die Eigenschaft nicht angezeigt werden,
        //    // wird nicht in die Liste aufgenommen
        //    if (eb_attr != null && !eb_attr.Browsable)
        //        continue;

        //    if (c_attr != null)
        //    {
        //        if (!sorted_properties.ContainsKey(c_attr.Name))
        //            sorted_properties[c_attr.Name] = new();

        //        sorted_properties[c_attr.Name].Add(property);
        //    }
        //    else
        //    {
        //        sorted_properties[""].Add(property);
        //    }
        //}

        //// Sortiere die Kategorie nach Buchstaben
        //var categories = sorted_properties.Keys.ToList();

        //// Verschiebe eine Kategorie der sonstigen Eigenschaften zum letzten Index
        //var misc_category = categories[0]; // Siehe oben zu erste Stelle als Misc 
        //categories.RemoveAt(0);

        //// Die Kategorie wird vom Benutzer nochmals gefiltert oder neusortiert.
        //if (CategoryFilterCallback != null)
        //{
        //    // Filtere oder ordnete die Kategorien durch Benutzer neu
        //    categories = new List<string>(CategoryFilterCallback.Invoke(categories.ToArray()));
        //}
        //else
        //{
        //    categories = categories.OrderBy(x => x).ToList();
        //}

        //// Füge die sonstige Kategorie danach als Letzte hinzu
        //categories.Add(misc_category);

        //// Liste jetzt die Eigenschaften auf
        //foreach (var sorted_category in categories)
        //{
        //    var prop = sorted_properties[sorted_category];

        //    if (prop.Count == 0)
        //        continue;

        //    var c_lbl = new Label()
        //    {
        //        Text = sorted_category == "" ? "Misc" : sorted_category,
        //        Background = new SolidBrush(Color.Gray),
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        Padding = new(2)
        //    };

        //    pnl.Widgets.Add(c_lbl);

        //    foreach (var property in prop)
        //    {
        //        var d_attr = property.GetCustomAttribute<DisplayNameAttribute>();

        //        var line = new VerticalStackPanel()
        //        {
        //            Margin = new(10, 0, 10, 0),
        //            Spacing = 2
        //        };

        //        line.Widgets.Add(new Label()
        //        {
        //            Text = ToReadableCase(d_attr != null ? d_attr.DisplayName : property.Name)
        //        });

        //        var editor_found = false;

        //        // 1) Prüft es, ob der Eigenschaft einen benutzerdefinierten Editor hat.
        //        if (_custom_editor_types != null)
        //        {
        //            foreach (var editor in _custom_editor_types)
        //            {
        //                if (AddEditor(line, property, editor))
        //                {
        //                    editor_found = true;
        //                    break;
        //                }
        //            }
        //        }

        //        // 2) Falls nicht, dann prüft es, ob der Eigenschaft einen integrierten Editor hat.
        //        if (!editor_found)
        //        {
        //            if (_editor_types == null)
        //            {
        //                _editor_types = new();
        //                _editor_types.AddRange(GetEditorTypes());
        //            }

        //            foreach (var editor in _editor_types)
        //            {
        //                if (AddEditor(line, property, editor))
        //                {
        //                    editor_found = true;
        //                    break;
        //                }
        //            }
        //        }

        //        // 3) Sonst kann diese Eigenschaft ohne Editor nicht geändert werden
        //        if (!editor_found)
        //        {
        //            line.Widgets.Add(new Widget());
        //        }

        //        pnl.Widgets.Add(line);
        //    }
        //}

        //owner.Content = pnl;
    }

    private bool AddEditor(VerticalStackPanel line, PropertyInfo property, PropertyEditorBase editor)
    {
        //if (editor.ForType(GetActuallyType(property.PropertyType)))
        //{
        //    var new_editor = (PropertyEditorBase)Activator.CreateInstance(editor.GetType())!;
        //    //new_editor._member = property;
        //    //new_editor._source = _source;
        //    new_editor._builder = this;
        //    Initialize?.Invoke(new_editor);
        //    new_editor.Initialize();
        //    line.Widgets.Add(new_editor.CreateControl());
        //    return true;
        //}
        return false;
    }

    private Type GetActuallyType(Type type)
    {
        var result_type = Nullable.GetUnderlyingType(type);

        if (result_type != null)
        {
            return result_type;
        }

        return type;
    }

    private string ToReadableCase(string property_name)
    {
        var len = property_name.Length;
        var output = "";

        for (int i = 0; i < len; i++)
        {
            var c = property_name[i];

            if (i == len - 1)
            {
                output += c;
                break;
            }

            var nx = property_name[i + 1];

            if (char.IsUpper(c) && i != 0 && !char.IsUpper(nx))
            {
                output += " ";
                output += c;
            }
            else if (!char.IsUpper(c) && i != 0 && char.IsUpper(nx))
            {
                output += c;
                output += " ";
            }
            else
            {
                output += c;
            }

        }

        return output;
    }
}
