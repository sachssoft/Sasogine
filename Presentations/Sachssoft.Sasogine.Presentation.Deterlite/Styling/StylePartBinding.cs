namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    public sealed class StyleBinding
    {
        // Typisch xml
        // z.B. <Button Background="{ExampleBrush}" />

        public StyleBinding(string name)
        {
        }

        public object? Resolve(Skin sheet)
        {
            // Hier müsste die Logik implementiert werden, um den gebundenen Stilteil anhand des Namens zu finden
            // Zum Beispiel könnte
            return default;
        }
    }
}
