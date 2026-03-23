using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components;
using System;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Inspection
{
    public abstract class DataInspection : NotifyingElement, IElementDefinition
    {
        public event EventHandler<DefinitionChangedEventArgs>? Changed;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            var change = History.GetLastChange();

            if (!change.HasValue)
                return;

            Changed?.Invoke(this, new DefinitionChangedEventArgs()
            {
                Key = change.Value.Property.Name,
                OldValue = change.Value.OldValue,
                NewValue = change.Value.NewValue
            });
        }
    }
}
