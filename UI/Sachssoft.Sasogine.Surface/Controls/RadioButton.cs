using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Styles;

namespace Sachssoft.Sasogine.Surface.Controls;

public class RadioButton : CheckBox, IGroupedCheckable
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

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not RadioButton source)
            return;

        GroupName = source.GroupName;
    }

    protected override ElementBase CreateCloneInstance()
    {
        return new RadioButton();
    }

    #endregion

    #region IGroupSelectable

    string? IGroupedCheckable.GroupName
    {
        get => GroupName.Value;
        set => GroupName = new StyleProperty<string?>(value, isUserSet: true);
    }

    #endregion
}