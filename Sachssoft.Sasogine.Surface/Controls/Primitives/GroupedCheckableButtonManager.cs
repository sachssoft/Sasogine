using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public static class GroupedCheckableButtonManager
    {
        private static readonly List<IGroupedCheckable> _registeredButtons = new();

        public static void Register(IGroupedCheckable button)
        {
            if (!_registeredButtons.Contains(button))
                _registeredButtons.Add(button);
        }

        public static void Unregister(IGroupedCheckable button)
        {
            if (_registeredButtons.Contains(button))
                _registeredButtons.Remove(button);
        }

        public static IEnumerable<IGroupedCheckable> GetButtonsByGroup(string? groupName)
            => _registeredButtons.Where(b => b.GroupName == groupName);

        public static void Check(IGroupedCheckable selectable)
        {
            //if (!selectable.IsLoaded)
            //    return;

            foreach (var b in GetButtonsByGroup(selectable.GroupName))
            {
                b.IsChecked = false;
            }

            selectable.IsChecked = true;
        }
    }
}
