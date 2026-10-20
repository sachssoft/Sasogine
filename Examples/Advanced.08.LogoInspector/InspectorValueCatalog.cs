using Microsoft.Xna.Framework;
using Sachssoft.Engine.Gameplay;
using Sachssoft.Engine.Graphics;
using Sachssoft.Engine.Graphics.Colors;
using Sachssoft.Engine.Geometry;
using Sachssoft.Engine.Resources;
using BigInteger = System.Numerics.BigInteger;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Provides representative values for every built-in factory through explicit generic construction.</summary>
internal static class InspectorValueCatalog
{
    internal static IReadOnlyList<IInspectorValueExample> Create()
    {
        var values = new List<IInspectorValueExample>();
        values.Add(new InspectorValueExample<Point>("Point", new Point(16, 24)));
        values.Add(new InspectorValueExample<Rectangle>("Rectangle", new Rectangle(16, 24, 128, 64)));
        values.Add(new InspectorValueExample<Vector2>("Vector2", new Vector2(12, 24)));
        values.Add(new InspectorValueExample<Vector3>("Vector3", new Vector3(12, 24, 36)));
        values.Add(new InspectorValueExample<Vector4>("Vector4", new Vector4(1, 2, 3, 4)));
        values.Add(new InspectorValueExample<Quaternion>("Quaternion", Quaternion.Identity));
        values.Add(new InspectorValueExample<Point2>("Point2", new Point2(16, 24)));
        values.Add(new InspectorValueExample<Point3>("Point3", new Point3(16, 24, 32)));
        values.Add(new InspectorValueExample<PixelPoint2>("PixelPoint2", new PixelPoint2(16, 24)));
        values.Add(new InspectorValueExample<PixelPoint3>("PixelPoint3", new PixelPoint3(16777217, 24, 32)));
        values.Add(new InspectorValueExample<Size2>("Size2", new Size2(128, 64)));
        values.Add(new InspectorValueExample<Size3>("Size3", new Size3(128, 64, 32)));
        values.Add(new InspectorValueExample<PixelSize2>("PixelSize2", new PixelSize2(128, 64)));
        values.Add(new InspectorValueExample<PixelSize3>("PixelSize3", new PixelSize3(128, 64, 32)));
        values.Add(new InspectorValueExample<Coordinate2>("Coordinate2", new Coordinate2(16, 24)));
        values.Add(new InspectorValueExample<Coordinate3>("Coordinate3", new Coordinate3(16, 24, 32)));
        values.Add(new InspectorValueExample<Insets2>("Insets2", new Insets2(1, 2, 3, 4)));
        values.Add(new InspectorValueExample<Insets3>("Insets3", new Insets3(1, 2, 3, 4, 5, 6)));
        values.Add(new InspectorValueExample<PixelInsets2>("PixelInsets2", new PixelInsets2(1, 2, 3, 4)));
        values.Add(new InspectorValueExample<PixelInsets3>("PixelInsets3", new PixelInsets3(1, 2, 3, 4, 5, 6)));
        values.Add(new InspectorValueExample<Bounds2>("Bounds2", new Bounds2(10, 20, 128, 64)));
        values.Add(new InspectorValueExample<Bounds3>("Bounds3", new Bounds3(10, 20, 30, 128, 64, 32)));
        values.Add(new InspectorValueExample<PixelBounds2>("PixelBounds2", new PixelBounds2(10, 20, 128, 64)));
        values.Add(new InspectorValueExample<PixelBounds3>("PixelBounds3", new PixelBounds3(10, 20, 30, 128, 64, 32)));
        values.Add(new InspectorValueExample<Box2>("Box2", new Box2(1, 2, 100, 200)));
        values.Add(new InspectorValueExample<Box3>("Box3", new Box3(1, 2, 3, 100, 200, 300)));
        values.Add(new InspectorValueExample<PixelBox2>("PixelBox2", new PixelBox2(1, 2, 100, 200)));
        values.Add(new InspectorValueExample<PixelBox3>("PixelBox3", new PixelBox3(1, 2, 3, 100, 200, 300)));
        values.Add(new InspectorValueExample<NormalizedColor>("NormalizedColor", new NormalizedColor(.2f, .4f, .6f, 1)));
        values.Add(new InspectorValueExample<RgbColor>("RgbColor", new RgbColor(.2f, .4f, .6f, 1)));
        values.Add(new InspectorValueExample<LinearRgbColor>("LinearRgbColor", new LinearRgbColor(.2f, .4f, .6f, 1)));
        values.Add(new InspectorValueExample<HslColor>("HslColor", new HslColor(210, .6f, .5f, 1)));
        values.Add(new InspectorValueExample<HsvColor>("HsvColor", new HsvColor(210, .6f, .8f, 1)));
        values.Add(new InspectorValueExample<OklabColor>("OklabColor", new OklabColor(.7f, .05f, -.1f, 1)));
        values.Add(new InspectorValueExample<Normalized>("Normalized", new Normalized(.5f)));
        values.Add(new InspectorValueExample<SignedNormalized>("SignedNormalized", new SignedNormalized(.25f)));
        values.Add(new InspectorValueExample<LifeInfo>("LifeInfo", new LifeInfo(75, 100)));
        values.Add(new InspectorValueExample<CollectibleInfo>("CollectibleInfo", new CollectibleInfo(3, 10)));
        values.Add(new InspectorValueExample<Segment>("Segment", new Segment(0, 0, 100, 50)));
        values.Add(new InspectorValueExample<Matrix>("Matrix", Matrix.Identity));
        values.Add(new InspectorValueExample<BoundingBox>("BoundingBox", new BoundingBox(new Vector3(-1), new Vector3(1))));
        values.Add(new InspectorValueExample<Ray>("Ray", new Ray(Vector3.Zero, Vector3.Forward)));
        values.Add(new InspectorValueExample<BoundingSphere>("BoundingSphere", new BoundingSphere(Vector3.Zero, 100)));
        values.Add(new InspectorValueExample<Plane>("Plane", new Plane(Vector3.Up, 0)));
        values.Add(new InspectorValueExample<FrameData>("FrameData", new FrameData(new PixelPoint2(16, 24), new PixelSize2(128, 64))));
        values.Add(new InspectorValueExample<TileFrameData>("TileFrameData", new TileFrameData(new PixelSize2(32, 32), new Coordinate2(2, 3))));
        values.Add(new InspectorValueExample<string>("String", "Sasogine"));
        values.Add(new InspectorValueExample<bool>("Boolean", true));
        values.Add(new InspectorValueExample<Color>("Color", new Color(65, 199, 223)));
        AddNumber<sbyte>(values, "sbyte");
        AddNumber<byte>(values, "byte");
        AddNumber<short>(values, "short");
        AddNumber<ushort>(values, "ushort");
        AddNumber<int>(values, "int");
        AddNumber<uint>(values, "uint");
        AddNumber<long>(values, "long");
        AddNumber<ulong>(values, "ulong");
        AddNumber<nint>(values, "nint");
        AddNumber<nuint>(values, "nuint");
        AddNumber<Int128>(values, "Int128");
        AddNumber<UInt128>(values, "UInt128");
        AddNumber<Half>(values, "Half");
        AddNumber<float>(values, "float");
        AddNumber<double>(values, "double");
        AddNumber<decimal>(values, "decimal");
        AddNumber<BigInteger>(values, "BigInteger");
        AddScores<int>(values, "int");
        AddScores<float>(values, "float");
        values.Add(new InspectorValueExample<TimeSpan>("TimeSpan", TimeSpan.FromMinutes(5)));
        values.Add(new InspectorValueExample<DateTime>("DateTime", new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)));
        values.Add(new InspectorValueExample<DateTimeOffset>("DateTimeOffset", new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero)));
        values.Add(new InspectorValueExample<Guid>("Guid", Guid.Parse("01234567-89ab-cdef-0123-456789abcdef")));
        values.Add(new InspectorValueExample<CountdownInfo>("CountdownInfo", new CountdownInfo(60)));
        values.Add(new InspectorValueExample<NamedGameRegistryKey>("NamedGameRegistryKey", new NamedGameRegistryKey("example")));
        values.Add(new InspectorValueExample<SortDirection>("SortDirection", SortDirection.Ascending));
        values.Add(new InspectorValueExample<ValueMode>("ValueMode", ValueMode.Absolute));
        values.Add(new InspectorValueExample<SwitchState>("SwitchState", SwitchState.On));
        values.Add(new InspectorValueExample<InspectorExampleMode>("InspectorExampleMode", InspectorExampleMode.Read));
        return values;
    }

    private static void AddNumber<TNumber>(List<IInspectorValueExample> values, string name)
        where TNumber : struct, System.Numerics.INumber<TNumber>
    {
        values.Add(new InspectorValueExample<TNumber>(name, TNumber.CreateChecked(5)));
    }

    private static void AddScores<TNumber>(List<IInspectorValueExample> values, string name)
        where TNumber : struct, System.Numerics.INumber<TNumber>
    {
        values.Add(new InspectorValueExample<LowTieredScore<TNumber>>("LowTieredScore<" + name + ">", new(TNumber.CreateChecked(1), TNumber.CreateChecked(5), TNumber.CreateChecked(10))));
        values.Add(new InspectorValueExample<HighTieredScore<TNumber>>("HighTieredScore<" + name + ">", new(TNumber.CreateChecked(1), TNumber.CreateChecked(5), TNumber.CreateChecked(10))));
    }
}
