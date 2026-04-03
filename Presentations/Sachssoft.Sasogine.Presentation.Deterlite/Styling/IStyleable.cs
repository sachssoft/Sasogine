namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    public interface IStyleable
    {
        string? StyleName { get; set; }

        void ApplyFromStyle(Style? style);
    }
}
