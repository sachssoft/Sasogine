using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>Mutually exclusive selection, scoped to a parent or an explicit root-wide group name.</summary>
public class RadioButton : CheckBox
{
    /// <summary>Initializes a new radio button instance.</summary>
    public RadioButton() : base()
    {
    }

    /// <summary>Gets or sets group name.</summary>
    public string GroupName { get => ConfigGroupName; set => ConfigGroupName = value; }
    /// <inheritdoc/>
    protected override string DefaultStyleName => "RadioButton";

    /// <inheritdoc/>
    protected override void ChangeCheckedState() => IsChecked = true;
    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        base.OnUpdate(elapsed);
        if (IsChecked == true)
            OnCheckedChanged();
    }

    /// <inheritdoc/>
    protected override void OnCheckedChanged()
    {
        if (IsChecked != true || Workspace is null)
            return;
        foreach (Control node in Workspace.EnumerateControls())
            if (node is RadioButton other && other != this && other.IsChecked == true && (GroupName.Length > 0 ? other.GroupName == GroupName : other.GroupName.Length == 0 && other.Parent == Parent))
                other.IsChecked = false;
    }

    private StyleProperty<string> _styleGroupName = new(string.Empty);
    private static readonly System.ComponentModel.PropertyChangedEventArgs GroupNameChangedArgs = new(nameof(GroupName));
    internal string ConfigGroupName
    {
        get => _styleGroupName.Value;
        set
        {
            if (_styleGroupName.SetLocal(value))
                NotifyPropertyChanged(GroupNameChangedArgs);
        }
    }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "GroupName":
                    if (_styleGroupName.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(GroupNameChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "GroupName":
                return _styleGroupName.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "GroupName":
                if (_styleGroupName.ClearLocal())
                    NotifyPropertyChanged(GroupNameChangedArgs);
                return;
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        if (_styleGroupName.BeginStyle(string.Empty))
            NotifyPropertyChanged(GroupNameChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleGroupName.CommitStyle())
            NotifyPropertyChanged(GroupNameChangedArgs);
    }
}
