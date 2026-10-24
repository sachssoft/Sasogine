# Switch

Ein eigenständiger mobiler Kippschalter auf Basis von `ToggleButton`. Die übrigen Controls behalten ihre Implementierung.

```csharp
var wifi = new Switch
{
    Content = "Wi-Fi",       // optional; auch eigene Controls möglich
    IsChecked = true,
    Command = toggleWifiCommand,
    CommandParameter = "wifi"
};

// Nach Workspace.Update:
if (wifi.IsPressed)
    ApplyWifiSetting(wifi.IsChecked);
```

- Touch-Tap, Mausklick, Leertaste und Enter verwenden die vorhandene Button-Aktivierung. Erst das Loslassen aktiviert; ein abgebrochener Touch schaltet nicht um.
- `IsChecked` bestimmt die linke/rechte Knopfposition. Es gibt keine frameabhängige Animation und kein automatisches Umschalten durch Hover.
- Standardmäßig mindestens 52 × 44 logische Pixel als Touchfläche, mit 52 × 28 großer Schiene und 24 großem Knopf. DPI übernimmt der Workspace.
- `TrackSize`, `ThumbSize`, `TextSpacing` und `ThumbColor` sind Stylewerte mit lokalen Overrides. Der Knopf muss in die Schienenhöhe passen.
- `Background` färbt die ausgeschaltete Schiene, `PressedBackground` die eingeschaltete. `DisabledBackground` und `DisabledTextColor` gelten im deaktivierten Zustand.
- Alle fünf Skins besitzen eigene `Switch`-Styles mit den Teilen `Track` und `Thumb` für Checked/Unchecked und Normal/Hover/Pressed/Disabled. Die Einträge verwenden vorhandene Atlasframes; bestehende Atlasbilder und Styles bleiben erhalten. Eigene Skins können die beiden Teile durch eigene Texturen ersetzen. Ohne Skin zeichnet das Control runde Formen direkt.
