namespace Sachssoft.Sasogine.Resources.Loaders
{
    public readonly struct LoaderResult
    {
        public bool Success { get; }

        public string? ErrorMessage { get; }

        public LoaderResult(bool success, string? errorMessage = null)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }

        public static LoaderResult Ok() => new(true);

        public static LoaderResult Fail(string error) => new(false, error);
    }
}
