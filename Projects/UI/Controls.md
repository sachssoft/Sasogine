# Controls-Übersicht

Aufbau und Migration: [Architektur.md](Architektur.md).

Neue Werkzeuge: [Dialoge, Diagramme, Expander, SplitButton und Script-Editor](Tools-Controls.md).

- TextBlock, Label und Image; ButtonBase mit Button, RepeatButton, ToggleButton, CheckBox, RadioButton und SelectorButton.
- TextBox, PasswordBox, NumericSpinner, Slider, RotationControl, Spinner, Calendar und DatePicker.
- ListBox, ComboBox, TreeView, DataTable, TabControl und TabItem.
- Panel, ContentControl, StackPanel, Grid, WrapPanel, TiledPanel und ScrollViewer mit separaten ScrollBars.
- MenuBar, ContextMenu, Window, Dialog, OpenFileDialog, FolderSelectorDialog, ColorPickerDialog, MessageBox und ToolTip.
- VirtualJoystick für freie/lineare und feste/freie Positionierung; CurveControl und EditableCurve für Kennlinien.

Alle Controls basieren auf Element und unterstützen INotifyPropertyChanged. Content-Controls können zusammengesetzte Controls aufnehmen. Commands und CommandParameter sind an den dafür vorgesehenen Controls verfügbar. Zeichnen erfolgt über ControlDrawContext und den zentralen XML-/Texture-Atlas-Skin.
