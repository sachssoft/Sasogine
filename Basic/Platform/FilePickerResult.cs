using System;

namespace Sachssoft.Sasogine.Platform
{
    public class FilePickerResult
    {
        /// <summary>
        /// True if user selected something, false if cancelled.
        /// </summary>
        public bool IsSuccess { get; init; }

        /// <summary>
        /// Paths selected by the user. May contain multiple entries if AllowMultiple is true.
        /// </summary>
        public string[] Paths { get; init; } = Array.Empty<string>();

    }
}
