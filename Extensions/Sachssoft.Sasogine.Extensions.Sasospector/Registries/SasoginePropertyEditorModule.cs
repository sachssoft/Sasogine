using Sachssoft.Sasogine.Common;
using Sachssoft.Sasospector.Adapters;
using System.Numerics;

namespace Sachssoft.Sasospector.Registries
{
    public sealed class SasoginePropertyEditorModule : IInspectorPropertyEditorModule
    {
        public void Register(InspectorPropertyEditorRegistryBase registry)
        {

            registry.RegisterType(typeof(Size),
                f => f.CreateMultipleValueEditor(
                    defaultDecimalPlaces: null,
                    adapter: new IndexedFieldPropertyAdapter<Size>(
                        uniformFieldType: typeof(float),
                        fieldCount: 2,
                        castTo: x => [
                                        new BoundedValue<float>(x.Width, float.MinValue, float.MaxValue),
                                        new BoundedValue<float>(x.Height, float.MinValue, float.MaxValue)
                                     ],
                        castFrom: y => new Size(
                                            width: (float)y[0],
                                            height: (float)y[1]
                                         )
                    )),
                isFallback: true);

            registry.RegisterType(typeof(PixelSize),
                f => f.CreateMultipleValueEditor(
                    defaultDecimalPlaces: null,
                    adapter: new IndexedFieldPropertyAdapter<PixelSize>(
                        uniformFieldType: typeof(int),
                        fieldCount: 2,
                        castTo: x => [
                                        new BoundedValue<int>(x.Width, int.MinValue, int.MaxValue),
                                        new BoundedValue<int>(x.Height, int.MinValue, int.MaxValue)
                                     ],
                        castFrom: y => new PixelSize(
                                            width: (int)y[0],
                                            height: (int)y[1]
                                         )
                    )),
                isFallback: true);

            registry.RegisterType(typeof(Insets),
                f => f.CreateMultipleValueEditor(
                    defaultDecimalPlaces: null,
                    adapter: new IndexedFieldPropertyAdapter<Insets>(
                        uniformFieldType: typeof(float),
                        fieldCount: 4,
                        castTo: x => [
                                        new BoundedValue<float>(x.Left, float.MinValue, float.MaxValue),
                                        new BoundedValue<float>(x.Top, float.MinValue, float.MaxValue),
                                        new BoundedValue<float>(x.Right, float.MinValue, float.MaxValue),
                                        new BoundedValue<float>(x.Bottom, float.MinValue, float.MaxValue)
                                     ],
                        castFrom: y => new Insets(
                                            left: (float)y[0],
                                            top: (float)y[1],
                                            right: (float)y[2],
                                            bottom: (float)y[3]
                                         )
                    )),
                isFallback: true);

            registry.RegisterType(typeof(PixelInsets),
                f => f.CreateMultipleValueEditor(
                    defaultDecimalPlaces: null,
                    adapter: new IndexedFieldPropertyAdapter<PixelInsets>(
                        uniformFieldType: typeof(int),
                        fieldCount: 4,
                        castTo: x => [
                                        new BoundedValue<int>(x.Left, int.MinValue, int.MaxValue),
                                        new BoundedValue<int>(x.Top, int.MinValue, int.MaxValue),
                                        new BoundedValue<int>(x.Right, int.MinValue, int.MaxValue),
                                        new BoundedValue<int>(x.Bottom, int.MinValue, int.MaxValue)
                                     ],
                        castFrom: y => new PixelInsets(
                                            left: (int)y[0],
                                            top: (int)y[1],
                                            right: (int)y[2],
                                            bottom: (int)y[3]
                                         )
                    )),
                isFallback: true);

            registry.RegisterType(typeof(Bounds),
                f => f.CreateMultipleValueEditor(
                    defaultDecimalPlaces: null,
                    adapter: new IndexedFieldPropertyAdapter<Bounds>(
                        uniformFieldType: typeof(float),
                        fieldCount: 4,
                        castTo: x => [
                                        new BoundedValue<float>(x.X, float.MinValue, float.MaxValue),
                                        new BoundedValue<float>(x.Y, float.MinValue, float.MaxValue),
                                        new BoundedValue<float>(x.Width, float.MinValue, float.MaxValue),
                                        new BoundedValue<float>(x.Height, float.MinValue, float.MaxValue)
                                     ],
                        castFrom: y => new Bounds(
                                            x: (float)y[0],
                                            y: (float)y[1],
                                            width: (float)y[2],
                                            height: (float)y[3]
                                         )
                    )),
                isFallback: true);

            registry.RegisterType(typeof(PixelBounds),
                f => f.CreateMultipleValueEditor(
                    defaultDecimalPlaces: null,
                    adapter: new IndexedFieldPropertyAdapter<PixelBounds>(
                        uniformFieldType: typeof(int),
                        fieldCount: 4,
                        castTo: x => [
                                        new BoundedValue<int>(x.X, int.MinValue, int.MaxValue),
                                        new BoundedValue<int>(x.Y, int.MinValue, int.MaxValue),
                                        new BoundedValue<int>(x.Width, int.MinValue, int.MaxValue),
                                        new BoundedValue<int>(x.Height, int.MinValue, int.MaxValue)
                                     ],
                        castFrom: y => new PixelBounds(
                                            x: (int)y[0],
                                            y: (int)y[1],
                                            width: (int)y[2],
                                            height: (int)y[3]
                                         )
                    )),
                isFallback: true);

            registry.RegisterType(typeof(Box),
                f => f.CreateMultipleValueEditor(
                    defaultDecimalPlaces: null,
                    adapter: new IndexedFieldPropertyAdapter<Box>(
                        uniformFieldType: typeof(float),
                        fieldCount: 4,
                        castTo: x => [
                                        new BoundedValue<float>(x.MinX, float.MinValue, float.MaxValue),
                                        new BoundedValue<float>(x.MinY, float.MinValue, float.MaxValue),
                                        new BoundedValue<float>(x.MaxX, float.MinValue, float.MaxValue),
                                        new BoundedValue<float>(x.MaxY, float.MinValue, float.MaxValue)
                                     ],
                        castFrom: y => new Box(
                                            minX: (float)y[0],
                                            minY: (float)y[1],
                                            maxX: (float)y[2],
                                            maxY: (float)y[3]
                                         )
                    )),
                isFallback: true);

            registry.RegisterType(typeof(PixelBox),
                f => f.CreateMultipleValueEditor(
                    defaultDecimalPlaces: null,
                    adapter: new IndexedFieldPropertyAdapter<PixelBox>(
                        uniformFieldType: typeof(int),
                        fieldCount: 4,
                        castTo: x => [
                                        new BoundedValue<int>(x.MinX, int.MinValue, int.MaxValue),
                                        new BoundedValue<int>(x.MinY, int.MinValue, int.MaxValue),
                                        new BoundedValue<int>(x.MaxX, int.MinValue, int.MaxValue),
                                        new BoundedValue<int>(x.MaxY, int.MinValue, int.MaxValue)
                                     ],
                        castFrom: y => new PixelBox(
                                            minX: (int)y[0],
                                            minY: (int)y[1],
                                            maxX: (int)y[2],
                                            maxY: (int)y[3]
                                         )
                    )),
                isFallback: true);

            registry.RegisterType(typeof(Coordinate2),
                f => f.CreateMultipleValueEditor(
                    defaultDecimalPlaces: null,
                    adapter: new IndexedFieldPropertyAdapter<Coordinate2>(
                        uniformFieldType: typeof(int),
                        fieldCount: 2,
                        castTo: x => [
                                        new BoundedValue<int>(x.X, int.MinValue, int.MaxValue),
                                        new BoundedValue<int>(x.Y, int.MinValue, int.MaxValue)
                                     ],
                        castFrom: y => new Coordinate2(
                                            x: (int)y[0],
                                            y: (int)y[1]
                                         )
                    )),
                isFallback: true);

            registry.RegisterType(typeof(Coordinate3),
                f => f.CreateMultipleValueEditor(
                    defaultDecimalPlaces: null,
                    adapter: new IndexedFieldPropertyAdapter<Coordinate3>(
                        uniformFieldType: typeof(int),
                        fieldCount: 3,
                        castTo: x => [
                                        new BoundedValue<int>(x.X, int.MinValue, int.MaxValue),
                                        new BoundedValue<int>(x.Y, int.MinValue, int.MaxValue),
                                        new BoundedValue<int>(x.Z, int.MinValue, int.MaxValue)
                                     ],
                        castFrom: y => new Coordinate3(
                                            x: (int)y[0],
                                            y: (int)y[1],
                                            z: (int)y[2]
                                         )
                    )),
                isFallback: true);
        }
    }
}
