using System;
using System.Globalization;
using System.Text;

namespace Sachssoft.Engine.Controls.Drawing;
internal static class ControlTextLayout
{
    internal static string Wrap(IControlFont font, string text, float width)
    {
        if (!float.IsFinite(width) || width <= 0)
            return text;
        var result = new StringBuilder();
        string[] paragraphs = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        for (int p = 0; p < paragraphs.Length; p++)
        {
            if (p > 0)
                result.Append('\n');
            string paragraph = paragraphs[p];
            int start = 0, lastSpace = -1;
            int[] boundaries = StringInfo.ParseCombiningCharacters(paragraph);
            foreach (int index in boundaries)
            {
                if (char.IsWhiteSpace(paragraph[index]))
                    lastSpace = index;
                int next = index + 1;
                while (next < paragraph.Length && Array.BinarySearch(boundaries, next) < 0)
                    next++;
                if (index > start && font.Measure(paragraph[start..next]).Width > width)
                {
                    int end = lastSpace > start ? lastSpace : index;
                    result.Append(paragraph.AsSpan(start, end - start));
                    result.Append('\n');
                    start = end;
                    while (start < paragraph.Length && paragraph[start] == ' ')
                        start++;
                    lastSpace = -1;
                }
            }

            result.Append(paragraph.AsSpan(start));
        }

        return result.ToString();
    }
}
