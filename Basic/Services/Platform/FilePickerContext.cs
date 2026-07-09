namespace Sachssoft.Sasogine.Services.Platform
{
    public class FilePickerContext
    {

        public string? Title { get; init; }

        public string? InitialDirectory { get; init; }

        public string? DefaultFileName { get; init; }

        public string[]? Filters { get; init; } // e.g., { "*.txt", "*.png" }

        public bool AllowMultiple { get; init; } = false;

    }
}
