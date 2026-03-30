namespace Sachssoft.Sasogine.Presentation.Deterlite
{
    internal interface IFrameChildHostInternal : IFrameChildHost
    {
        IFrameChildHost? Parent { get; set; }

    }
}
