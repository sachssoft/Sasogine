using System.Text;
using System.Text.RegularExpressions;

namespace sachssoft.Sasogine.Document;

internal static class JsonUtils
{
    // PascalCase -> train-case (lowercase, mit Bindestrichen)
    public static string PascalToTrainCase(string pascal_case)
    {
        if (string.IsNullOrEmpty(pascal_case))
            return pascal_case;

        // Trenne vor Großbuchstaben (außer am Anfang), dann alles lowercase und mit '-'
        var result = Regex.Replace(pascal_case, "(?<!^)([A-Z])", "-$1").ToLower();
        return result;
    }

    // train-case -> PascalCase (Wörter mit Bindestrich zu großem Anfangsbuchstaben zusammenfügen)
    public static string TrainToPascalCase(string train_case)
    {
        if (string.IsNullOrEmpty(train_case))
            return train_case;

        var parts = train_case.Split('-');
        var sb = new StringBuilder();
        foreach (var part in parts)
        {
            if (part.Length == 0)
                continue;
            sb.Append(char.ToUpper(part[0]));
            if (part.Length > 1)
                sb.Append(part.Substring(1));
        }
        return sb.ToString();
    }
}
