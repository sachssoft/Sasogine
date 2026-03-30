using System;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Surface.Behaviors
{
    public static class PropertyObserver
    {
        public static PropertyObserver<T> Create<T>(T source) where T : class, INotifyPropertyChanged
            => new PropertyObserver<T>(source);
    }
}
