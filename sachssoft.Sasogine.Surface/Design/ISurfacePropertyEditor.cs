using Sachssoft.Sasogine.Surface.Visuals.Controls;
using System;

namespace Sachssoft.Sasogine.Surface.Design;

public interface ISurfacePropertyEditor : IPropertyEditor
{
    Widget CreateControl<T, TVal>(T source, Action<T, string> changed, Func<T, TVal> getter, Action<T, TVal> setter);
}
