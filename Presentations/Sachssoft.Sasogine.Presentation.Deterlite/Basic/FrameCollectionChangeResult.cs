using System;
using System.Linq;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Basic
{
    public readonly struct FrameCollectionChangeResult
    {
        public static readonly FrameCollectionChangeResult None =
            new FrameCollectionChangeResult(Array.Empty<FrameBase>(), FrameCollectionChangeType.None);

        public FrameCollectionChangeResult(FrameBase[] frames, FrameCollectionChangeType state)
        {
            Frames = frames ?? Array.Empty<FrameBase>();
            State = state;
        }

        public FrameBase[] Frames { get; }
        public int Count => Frames.Length;
        public FrameCollectionChangeType State { get; }
    }
}
