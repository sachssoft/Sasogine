namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public interface IGroupedCheckable : ICheckable
    {
        string? GroupName { get; set; }
    }
}
