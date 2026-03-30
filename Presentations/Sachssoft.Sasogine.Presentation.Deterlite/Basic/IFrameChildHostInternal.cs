namespace Sachssoft.Sasogine.Presentation.Deterlite.Basic
{
    internal interface IFrameChildHostInternal : IFrameChildHost
    {
        IFrameChildHost? Parent { get; set; }

    }
}
