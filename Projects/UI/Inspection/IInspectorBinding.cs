using Sachssoft.Engine.Controls.Binding;

namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>
/// A binding whose notification lifetime follows its inspector.
/// </summary>
internal interface IInspectorBinding : IFrameBinding
{
    void Connect();
    void Disconnect();
}
