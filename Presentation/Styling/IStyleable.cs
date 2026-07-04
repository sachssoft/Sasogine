namespace Sachssoft.Sasogine.Presentation.Styling
{
    public interface IStyleable
    {
        string? StyleName { get; set; }

        void ApplyFromStyle(Style? style);
    }
}
