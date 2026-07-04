namespace Sachssoft.Sasogine.Presentation
{
    internal interface IFrameChildHostInternal : IFrameChildHost
    {
        IFrameChildHost? Parent { get; set; }

    }
}
