using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;
using Sachssoft.Engine.Controls.Styling;
using Sachssoft.Engine.Graphics.Text;

/// <summary>Checks family resolution, inheritance, style overrides and shared cache lifetime.</summary>
internal static class FontControlChecks
{
    internal static int Run()
    {
        int checks = 0;
        void Check(bool ok, string message)
        {
            checks++;
            if (!ok)
                throw new InvalidOperationException(message);
        }

        FontFace Face(string name, FontWeight weight = FontWeight.Normal, FontStyle style = FontStyle.Normal) => new(new byte[] { 0 }, name, weight, style);
        var regular = Face("Regular");
        var bold = Face("Bold", FontWeight.Bold);
        var italic = Face("Italic", style: FontStyle.Italic);
        var both = Face("Bold Italic", FontWeight.Bold, FontStyle.Italic);
        var fonts = new FontRegistry();
        fonts.Register("Test", regular, bold, italic, both);
        var created = new List<FontProbe>();
        var registry = new UIRegistry(fonts, (face, size) =>
        {
            var adapter = new FontProbe(face, size);
            created.Add(adapter);
            return adapter;
        });
        var root = new StackPanel
        {
            FontName = "Test",
            FontSize = 20
        };
        var inherited = new TextBlock
        {
            Text = "Hello"
        };
        var buttonText = new TextBlock
        {
            Text = "Button"
        };
        var button = new Button
        {
            Content = buttonText,
            FontWeight = FontWeight.Bold
        };
        var local = new TextBlock
        {
            Text = "Local",
            FontSize = 12,
            FontWeight = FontWeight.Normal
        };
        var editor = new TextBox
        {
            Text = "Text"
        };
        root.Add(inherited);
        root.Add(button);
        root.Add(local);
        root.Add(editor);
        var workspace = new Workspace(root, registry);
        var fallback = new FontProbe(regular, 16);
        var layout = new ControlLayoutContext(fallback);
        var frame = new ControlInputFrame(new Size2(600, 500), TimeSpan.Zero);
        void Frame() => workspace.Update(frame, layout);
        Frame();
        Check(ReferenceEquals(root.EffectiveFont, inherited.EffectiveFont), "Child inherits the shared adapter");
        Check(ReferenceEquals(editor.EffectiveFont, inherited.EffectiveFont), "TextBox shares the family adapter");
        Check(ReferenceEquals(button.EffectiveFont, buttonText.EffectiveFont), "Content inherits Button font");
        Check(((FontProbe)buttonText.EffectiveFont!).Face == bold, "Bold face selected");
        Check(created.Count == 3, "Exactly one adapter per face and size");
        float oldWidth = inherited.DesiredSize.Width;
        root.FontSize = 30;
        root.FontStyle = FontStyle.Italic;
        root.FontWeight = FontWeight.Bold;
        Frame();
        Check(((FontProbe)inherited.EffectiveFont!).Face == both, "Combined weight and style");
        Check(inherited.DesiredSize.Width > oldWidth, "Font changes invalidate text metrics");
        Check(((FontProbe)local.EffectiveFont!).Size == 12 && ((FontProbe)local.EffectiveFont!).Face == italic, "Local normal overrides inherited bold and inherits italic");
        local.FontSize = null;
        Frame();
        Check(((FontProbe)local.EffectiveFont!).Size == 30, "Null restores inheritance");
        var style = new Style
        {
            BasedOn = "TextBlock",
            FontSize = 18,
            FontWeight = FontWeight.Normal,
            FontStyle = FontStyle.Normal
        };
        registry.Styles.Register("Font.Sample", style);
        local.StyleId = "Font.Sample";
        Frame();
        Check(((FontProbe)local.EffectiveFont!).Size == 30, "Explicit local null suppresses style and inherits");
        local.ResetStyleProperty("FontSize");
        Frame();
        Check(((FontProbe)local.EffectiveFont!).Size == 18, "Reset uses central style value");
        style.FontSize = 22;
        Frame();
        Check(((FontProbe)local.EffectiveFont!).Size == 22, "Live style changes update fonts");
        var borrowed = new FontProbe(regular, 9);
        button.Font = borrowed;
        button.FontWeight = null;
        Frame();
        Check(ReferenceEquals(buttonText.EffectiveFont, borrowed), "Explicit adapter starts an inherited font scope");
        var popup = new TextBlock
        {
            Text = "Popup"
        };
        workspace.ShowPopup(popup, new Bounds2(0, 100, 100, 30), inherited);
        Frame();
        Check(ReferenceEquals(popup.EffectiveFont, inherited.EffectiveFont), "Popup inherits owner typography");
        for (int i = 0; i < 100; i++)
            Frame();
        int count = created.Count;
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            Frame();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Check(allocated == 0, "Unchanged named-font frames allocate zero bytes: " + allocated);
        Check(created.Count == count, "Unchanged frames create no adapters");
        var replacement = Face("Replacement", FontWeight.Bold, FontStyle.Italic);
        fonts.Remove("Test");
        fonts.Register("Test", replacement);
        inherited.Text = "New text during font replacement";
        workspace.InvalidateFonts();
        Frame();
        Check(((FontProbe)inherited.EffectiveFont!).Face == replacement, "Same-count replacement resolves after invalidation");
        Check(created.Take(count).All(f => f.Disposals == 1), "Invalidated adapters disposed exactly once");
        root.FontName = "Missing";
        bool failed = false;
        try
        {
            Frame();
        }
        catch (InvalidOperationException)
        {
            failed = true;
        }

        Check(failed, "Missing family fails clearly");
        fonts.Register("Missing", regular);
        Frame();
        Check(((FontProbe)inherited.EffectiveFont!).Face == regular, "Failed resolution retries after family registration");
        workspace.Dispose();
        Check(created.All(f => f.Disposals == 1), "Workspace releases all created adapters once");
        Check(fallback.Disposals == 0 && borrowed.Disposals == 0, "Caller-owned fonts remain borrowed");
        var converters = registry.Converters;
        Check(converters.Convert<FontWeight>("FontWeight", "700") == FontWeight.Bold, "Numeric weight converter");
        Check(converters.Convert<FontStyle?>("NullableFontStyle", "inherit")is null, "Inheritance converter");
        return checks;
    }
}
