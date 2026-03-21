using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Styles;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Surface.Controls;

public class ChoiceButton : ToggleButton, IGroupedCheckable
{
    private StyleProperty<string?> _groupName = new StyleProperty<string?>(null, isUserSet: false);

    #region Style Properties

    public StyleProperty<string?> GroupName
    {
        get => _groupName;
        set => SetAndNotify(ref _groupName, value);
    }

    #endregion

    protected override void OnLoaded()
    {
        base.OnLoaded();
        GroupedCheckableButtonManager.Register(this);
    }

    protected override void OnUnloaded()
    {
        GroupedCheckableButtonManager.Unregister(this);
        base.OnUnloaded();
    }

    protected override void OnPressedChangedByUser(ValueChangingEventArgs<bool> e)
    {
        // Wenn der Benutzer versucht, den bereits aktivierten Radiobutton zu deaktivieren:
        if (IsPressed.Value && !e.NewValue)
        {
            // Aktion abbrechen – RadioButtons bleiben aktiv, bis ein anderer gewählt wird
            e.Cancel = true;
            return;
        }

        base.OnPressedChangedByUser(e);

        // Nur bei Aktivierung auslösen
        if (e.NewValue)
        {
            GroupedCheckableButtonManager.Check(this);
        }
    }

    #region Style

    public override void ApplyFromStyle(Style? style)
    {
        base.ApplyFromStyle(style);

        style?.Apply(this, (target, sheet, property, value) =>
        {
            switch (property)
            {
                case nameof(GroupName):
                    target.GroupName = target.GroupName.Override(value.RawValue);
                    break;
            }
        });
    }

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not ChoiceButton source)
            return;

        GroupName = source.GroupName;
    }

    protected override ElementBase CreateCloneInstance()
    {
        return new ChoiceButton();
    }

    #endregion

    #region IGroupSelectable

    //bool IGroupedCheckable.IsChecked
    //{
    //    get => IsPressed.Value;
    //    set => IsPressed = new StyleProperty<bool>(value, isUserSet: true);
    //}

    string? IGroupedCheckable.GroupName
    {
        get => GroupName.Value;
        set => GroupName = new StyleProperty<string?>(value, isUserSet: true);
    }

    #endregion
}