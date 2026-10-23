using System;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// A dedicated masked editor. Passwords are normal managed strings; copy/cut are disabled.
/// </summary>
public class PasswordBox : TextBox
{
    /// <summary>
    /// Creates a password editor.
    /// </summary>
    public PasswordBox() : base()
    {
    }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get => Text; set => Text = value; }

    /// <summary>
    /// Occurs when the password changes.
    /// </summary>
    public event EventHandler? PasswordChanged { add => TextChanged += value; remove => TextChanged -= value; }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "PasswordBox";

    internal override void Prepare()
    {
        base.Prepare();
        ConfigIsPassword = true;
        ConfigAcceptsReturn = false;
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>
    /// Removes a local override and restores the current style value.
    /// </summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
    }
}
