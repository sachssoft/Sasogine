
namespace Sachssoft.Sasogine.Surface.Controls.Capture
{
    public interface ICaptureHost
    {
        ICaptureChild? CurrentChild { get; set; }

        bool IsCaptureEnabled { get; }
    }
}
