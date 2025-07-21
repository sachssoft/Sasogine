using sachssoft.Sasogine.Resources;
using System.Text.Json;

namespace sachssoft.Sasogine.Network;

public static class ApiReaderHelper
{
    public static LocalizedValue<string>? ReadLocalizedFromJson(string jsonString, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return null;

        using var doc = JsonDocument.Parse(jsonString);
        var root = doc.RootElement;

        // Die vorhandene Methode wiederverwenden
        return ReadLocalizedFromJson(root, propertyName);
    }

    public static LocalizedValue<string>? ReadLocalizedFromJson(JsonElement item, string propertyName)
    {
        LocalizedValue<string>? output = null;
        if (item.TryGetProperty(propertyName, out var descriptionJson))
        {
            // Prüfen, was wirklich drinsteckt
            switch (descriptionJson.ValueKind)
            {
                case JsonValueKind.Null:
                    // nichts vorhanden
                    break;

                case JsonValueKind.Object:
                    {
                        var localizedDescription = new LocalizedValue<string>();
                        foreach (var lang in descriptionJson.EnumerateObject())
                            localizedDescription.TryAdd(lang.Name, lang.Value.GetString());
                        output = localizedDescription;
                    }
                    break;

                case JsonValueKind.String:
                    // Hier ist der Inhalt ein "doppelt serialisiertes" JSON!
                    // Das passiert oft bei DB-Feldern wie "description_json" = "\"{\\\"en\\\":\\\"...\\\"}\""
                    var innerJson = descriptionJson.GetString();
                    if (!string.IsNullOrEmpty(innerJson))
                    {
                        // Einmalig neu parsen
                        using var innerDoc = JsonDocument.Parse(innerJson);
                        if (innerDoc.RootElement.ValueKind == JsonValueKind.Object)
                        {
                            var localizedDescription = new LocalizedValue<string>();
                            foreach (var lang in innerDoc.RootElement.EnumerateObject())
                                localizedDescription.TryAdd(lang.Name, lang.Value.GetString());
                            output = localizedDescription;
                        }
                    }
                    break;

                default:
                    // Irgendwas Unerwartetes → ignorieren
                    break;
            }
        }
        return output;
    }
}
