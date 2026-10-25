# Hyperlink

`Sachssoft.Engine.Controls.Hyperlink` derives from `ButtonBase`. Assign `Content` as for a button. Generated text uses the existing cached `TextBlock` layout with state colors and per-line underlines. No button background is drawn; keyboard focus remains visible.

```csharp
var link = new Hyperlink
{
    Content = "Sachssoft website",
    NavigateUri = new Uri("https://www.sachssoft.com/")
};
link.NavigateRequested += (_, _) => Navigate(link.NavigateUri!);
```

`Navigate` is the application's navigation function. The control never launches a browser or another process itself. Absolute and relative targets are supported, so the same event can route web links or internal application pages. With `NavigateUri = null`, the link supports ordinary command-only activation.

`Command`, `CommandParameter`, `Clicked`, `PerformClick()`, `IsHeld` and frame polling with `IsPressed` come from `ButtonBase`. Mouse, touch, Enter and Space use its existing press/release and cancellation rules. Disabled controls and commands whose `CanExecute` returns false do not activate.

Activation order is `Clicked`, command execution, `IsVisited = true`, then `NavigateRequested` when a target exists. Changing the URI's original text, including its fragment, resets `IsVisited`. Applications can also reset or set this state directly.

Central styles and local overrides support `TextColor`, `HoverTextColor`, `PressedTextColor`, `VisitedTextColor`, `DisabledTextColor` and `Underline`. Held state takes priority over hover/focus, followed by visited and normal colors. Disabled links use the disabled color. All five bundled skins provide link colors without requiring a new atlas texture.

Text content reuses one presenter across assignments. Underline widths are measured only with changed text, wrapping or font resources; state color changes retain geometry. To wrap generated text, configure its existing TextBlock presenter:

```csharp
link.Width = 240;
((TextBlock)link.ContentPresenter!).WrapText = true;
```

Explicit control content retains its own presentation and ownership. Automatic link colors and text underlines apply to generated text content; custom controls render their own appearance. `ContentControl.CreateTextPresenter` and the protected `TextBlock` text drawing hooks provide this specialization without introducing another text layout engine.
