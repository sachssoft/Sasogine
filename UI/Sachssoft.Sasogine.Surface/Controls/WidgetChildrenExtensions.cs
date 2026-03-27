using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public static class WidgetChildrenExtensions
    {
        public static TWidget? FindChildById<TWidget>(this Widget widget, string? id) where TWidget : Widget
        {
            return widget.FindChild<TWidget>(w => w.Id == id);
        }

        public static Widget? FindChildById(this Widget widget, string? id)
        {
            return widget.FindChild(w => w.Id == id);
        }

        public static TWidget? FindChild<TWidget>(this Widget widget, Func<TWidget, bool>? predicate) where TWidget : Widget
        {
            foreach (var child in widget.LayoutChildren)
            {
                if (child is TWidget casted && (predicate == null || predicate(casted)))
                    return casted;

                var result = child.FindChild(predicate);
                if (result != null) return result;
            }
            return null;
        }

        public static Widget? FindChild(this Widget widget, Func<Widget, bool>? predicate)
        {
            foreach (var child in widget.LayoutChildren)
            {
                if (predicate == null || predicate(child))
                    return child;

                var result = child.FindChild(predicate);
                if (result != null) return result;
            }
            return null;
        }

        public static IEnumerable<Widget> GetChildren(this Widget widget, bool recursive = false, Func<Widget, bool>? predicate = null)
        {
            foreach (var child in widget.LayoutChildren)
            {
                if (predicate == null || predicate(child))
                    yield return child;

                if (recursive)
                {
                    foreach (var inner in child.GetChildren(true, predicate))
                        yield return inner;
                }
            }
        }
    }
}
