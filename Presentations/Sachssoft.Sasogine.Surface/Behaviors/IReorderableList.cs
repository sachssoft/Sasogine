using System.Collections;

namespace Sachssoft.Sasogine.Surface.Behaviors
{
    public interface IReorderableList : IList
    {
        void Move(int fromIndex, int toIndex);

        void Swap(int indexA, int indexB);
    }
}
