using System.ComponentModel;
using Sachssoft.Engine.Controls;

internal sealed class ValidationModel : Element
{
    private string _name = "Initial";
    private static readonly PropertyChangedEventArgs NameArgs = new(nameof(Name));
    internal string Name { get => _name; set => SetNotify(ref _name, value, NameArgs); }
}
