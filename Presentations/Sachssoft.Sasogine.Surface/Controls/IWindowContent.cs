using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public interface IWindowContent
    {
        bool IsWindowHosted { get; }

        void Show(IWindowHost host);

        Task? ShowAsync(IWindowHost host);

        void Close();
    }
}
