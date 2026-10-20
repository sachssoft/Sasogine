namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>Parses a value explicitly without member discovery.</summary>
/// <typeparam name = "T">The value type.</typeparam>
/// <param name = "text">The input text.</param>
/// <param name = "value">The parsed value when successful.</param>
/// <returns>Whether parsing succeeded.</returns>
public delegate bool TryParseInspectorValue<T>(string text, out T value);
