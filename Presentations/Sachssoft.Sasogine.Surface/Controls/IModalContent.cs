using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public interface IModalContent
    {
        bool IsModalHosted { get; }

        IBrush? ModalBackground { get; set; }

        void ShowModal(IModalHost host, Action<ModalResult> result); 
        
        Task<ModalResult> ShowModalAsync(IModalHost host);

        void Close();
    }
}
