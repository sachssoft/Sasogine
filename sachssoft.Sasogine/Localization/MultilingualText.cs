using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace sachssoft.Sasogine.Localization;

public class MultilingualText
{
    private Dictionary<string, string?> _textes;


    public MultilingualText()
    {
        _textes = new();
    }

    public static MultilingualText Create() => new MultilingualText();

    public MultilingualText Add(string language_token, string? text)
    {
        _textes.Add(language_token, text);
        return this;
    }

    public MultilingualText(string json)
    {
        _textes = new();

        var jobj = JsonSerializer.Deserialize<JsonObject>(json);

        foreach (var item in jobj)
        {
            _textes.Add(item.Key, item.Value.AsValue().GetValue<string>());
        }
    }

    public string? GetString(string language_token, string? default_text = null)
    {
        if (_textes.TryGetValue(language_token, out var text))
            return text;

        return default_text;
    }
}
