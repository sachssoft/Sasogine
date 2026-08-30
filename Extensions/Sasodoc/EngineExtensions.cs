using FontStashSharp;
using Microsoft.Xna.Framework;
using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Gameplay;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
    public static class EngineExtensions
    {
        #region Size2
        public static Size2 ReadSize2(this FormatReaderBase reader, string property, Size2 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var width = childReader.ReadSingle(nameof(Size2.Width), fallback.Width);
            var height = childReader.ReadSingle(nameof(Size2.Height), fallback.Height);

            return (new Size2(width, height));
        }

        public static void WriteSize2(this FormatWriterBase writer, string property, Size2 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteSingle(nameof(Size2.Width), value.Width);
            childWriter.WriteSingle(nameof(Size2.Height), value.Height);

            writer.Write(property, childWriter);
        }
        #endregion

        #region Size3
        public static Size3 ReadSize3(this FormatReaderBase reader, string property, Size3 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var width = childReader.ReadSingle(nameof(Size3.Width), fallback.Width);
            var height = childReader.ReadSingle(nameof(Size3.Height), fallback.Height);
            var depth = childReader.ReadSingle(nameof(Size3.Depth), fallback.Depth);

            return (new Size3(width, height, depth));
        }

        public static void WriteSize3(this FormatWriterBase writer, string property, Size3 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteSingle(nameof(Size3.Width), value.Width);
            childWriter.WriteSingle(nameof(Size3.Height), value.Height);
            childWriter.WriteSingle(nameof(Size3.Depth), value.Depth);

            writer.Write(property, childWriter);
        }
        #endregion

        #region PixelSize2
        public static PixelSize2 ReadPixelSize2(this FormatReaderBase reader, string property, PixelSize2 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var width = childReader.ReadInt32(nameof(PixelSize2.Width), fallback.Width);
            var height = childReader.ReadInt32(nameof(PixelSize2.Height), fallback.Height);

            return (new PixelSize2(width, height));
        }

        public static void WritePixelSize2(this FormatWriterBase writer, string property, PixelSize2 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteInt32(nameof(PixelSize2.Width), value.Width);
            childWriter.WriteInt32(nameof(PixelSize2.Height), value.Height);

            writer.Write(property, childWriter);
        }
        #endregion

        #region PixelSize3
        public static PixelSize3 ReadPixelSize3(this FormatReaderBase reader, string property, PixelSize3 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var width = childReader.ReadInt32(nameof(PixelSize3.Width), fallback.Width);
            var height = childReader.ReadInt32(nameof(PixelSize3.Height), fallback.Height);
            var depth = childReader.ReadInt32(nameof(PixelSize3.Depth), fallback.Depth);

            return (new PixelSize3(width, height, depth));
        }

        public static void WritePixelSize3(this FormatWriterBase writer, string property, PixelSize3 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteInt32(nameof(PixelSize3.Width), value.Width);
            childWriter.WriteInt32(nameof(PixelSize3.Height), value.Height);
            childWriter.WriteInt32(nameof(PixelSize3.Depth), value.Depth);

            writer.Write(property, childWriter);
        }
        #endregion

        #region Insets2
        public static Insets2 ReadInsets2(this FormatReaderBase reader, string property, Insets2 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var left = childReader.ReadSingle(nameof(Insets2.Left), fallback.Left);
            var top = childReader.ReadSingle(nameof(Insets2.Top), fallback.Top);
            var right = childReader.ReadSingle(nameof(Insets2.Right), fallback.Right);
            var bottom = childReader.ReadSingle(nameof(Insets2.Bottom), fallback.Bottom);

            return (new Insets2(left, top, right, top));
        }

        public static void WriteInsets2(this FormatWriterBase writer, string property, Insets2 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteSingle(nameof(Insets2.Left), value.Left);
            childWriter.WriteSingle(nameof(Insets2.Top), value.Top);
            childWriter.WriteSingle(nameof(Insets2.Right), value.Right);
            childWriter.WriteSingle(nameof(Insets2.Bottom), value.Bottom);

            writer.Write(property, childWriter);
        }
        #endregion

        #region Insets3
        public static Insets3 ReadInsets3(this FormatReaderBase reader, string property, Insets3 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var left = childReader.ReadSingle(nameof(Insets3.Left), fallback.Left);
            var top = childReader.ReadSingle(nameof(Insets3.Top), fallback.Top);
            var front = childReader.ReadSingle(nameof(Insets3.Front), fallback.Front);
            var right = childReader.ReadSingle(nameof(Insets3.Right), fallback.Right);
            var bottom = childReader.ReadSingle(nameof(Insets3.Bottom), fallback.Bottom);
            var back = childReader.ReadSingle(nameof(Insets3.Back), fallback.Back);

            return (new Insets3(left, top, front, right, top, back));
        }

        public static void WriteInsets3(this FormatWriterBase writer, string property, Insets3 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteSingle(nameof(Insets3.Left), value.Left);
            childWriter.WriteSingle(nameof(Insets3.Top), value.Top);
            childWriter.WriteSingle(nameof(Insets3.Front), value.Front);
            childWriter.WriteSingle(nameof(Insets3.Right), value.Right);
            childWriter.WriteSingle(nameof(Insets3.Bottom), value.Bottom);
            childWriter.WriteSingle(nameof(Insets3.Back), value.Back);

            writer.Write(property, childWriter);
        }
        #endregion

        #region PixelInsets2
        public static PixelInsets2 ReadPixelInsets2(this FormatReaderBase reader, string property, PixelInsets2 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var left = childReader.ReadInt32(nameof(PixelInsets2.Left), fallback.Left);
            var top = childReader.ReadInt32(nameof(PixelInsets2.Top), fallback.Top);
            var right = childReader.ReadInt32(nameof(PixelInsets2.Right), fallback.Right);
            var bottom = childReader.ReadInt32(nameof(PixelInsets2.Bottom), fallback.Bottom);

            return (new PixelInsets2(left, top, right, top));
        }

        public static void WritePixelInsets2(this FormatWriterBase writer, string property, PixelInsets2 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteInt32(nameof(PixelInsets2.Left), value.Left);
            childWriter.WriteInt32(nameof(PixelInsets2.Top), value.Top);
            childWriter.WriteInt32(nameof(PixelInsets2.Right), value.Right);
            childWriter.WriteInt32(nameof(PixelInsets2.Bottom), value.Bottom);

            writer.Write(property, childWriter);
        }
        #endregion

        #region PixelInsets3
        public static PixelInsets3 ReadPixelInsets3(this FormatReaderBase reader, string property, PixelInsets3 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var left = childReader.ReadInt32(nameof(PixelInsets3.Left), fallback.Left);
            var top = childReader.ReadInt32(nameof(PixelInsets3.Top), fallback.Top);
            var front = childReader.ReadInt32(nameof(PixelInsets3.Front), fallback.Front);
            var right = childReader.ReadInt32(nameof(PixelInsets3.Right), fallback.Right);
            var bottom = childReader.ReadInt32(nameof(PixelInsets3.Bottom), fallback.Bottom);
            var back = childReader.ReadInt32(nameof(PixelInsets3.Back), fallback.Back);

            return (new PixelInsets3(left, top, front, right, top, back));
        }

        public static void WritePixelInsets3(this FormatWriterBase writer, string property, PixelInsets3 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteInt32(nameof(PixelInsets3.Left), value.Left);
            childWriter.WriteInt32(nameof(PixelInsets3.Top), value.Top);
            childWriter.WriteInt32(nameof(PixelInsets3.Front), value.Front);
            childWriter.WriteInt32(nameof(PixelInsets3.Right), value.Right);
            childWriter.WriteInt32(nameof(PixelInsets3.Bottom), value.Bottom);
            childWriter.WriteInt32(nameof(PixelInsets3.Back), value.Back);

            writer.Write(property, childWriter);
        }
        #endregion

        #region Bounds2
        public static Bounds2 ReadBounds2(this FormatReaderBase reader, string property, Bounds2 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var x = childReader.ReadSingle(nameof(Bounds2.X), fallback.X);
            var y = childReader.ReadSingle(nameof(Bounds2.Y), fallback.Y);
            var width = childReader.ReadSingle(nameof(Bounds2.Width), fallback.Width);
            var height = childReader.ReadSingle(nameof(Bounds2.Height), fallback.Height);

            return (new Bounds2(x, y, width, height));
        }

        public static void WriteBounds2(this FormatWriterBase writer, string property, Bounds2 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteSingle(nameof(Bounds2.X), value.X);
            childWriter.WriteSingle(nameof(Bounds2.Y), value.Y);
            childWriter.WriteSingle(nameof(Bounds2.Width), value.Width);
            childWriter.WriteSingle(nameof(Bounds2.Height), value.Height);

            writer.Write(property, childWriter);
        }
        #endregion

        #region Bounds3
        public static Bounds3 ReadBounds3(this FormatReaderBase reader, string property, Bounds3 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var x = childReader.ReadSingle(nameof(Bounds3.X), fallback.X);
            var y = childReader.ReadSingle(nameof(Bounds3.Y), fallback.Y);
            var z = childReader.ReadSingle(nameof(Bounds3.Z), fallback.Z);
            var width = childReader.ReadSingle(nameof(Bounds3.Width), fallback.Width);
            var height = childReader.ReadSingle(nameof(Bounds3.Height), fallback.Height);
            var depth = childReader.ReadSingle(nameof(Bounds3.Depth), fallback.Depth);

            return (new Bounds3(x, y, z, width, height, depth));
        }

        public static void WriteBounds3(this FormatWriterBase writer, string property, Bounds3 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteSingle(nameof(Bounds3.X), value.X);
            childWriter.WriteSingle(nameof(Bounds3.Y), value.Y);
            childWriter.WriteSingle(nameof(Bounds3.Z), value.Z);
            childWriter.WriteSingle(nameof(Bounds3.Width), value.Width);
            childWriter.WriteSingle(nameof(Bounds3.Height), value.Height);
            childWriter.WriteSingle(nameof(Bounds3.Depth), value.Depth);

            writer.Write(property, childWriter);
        }
        #endregion

        #region PixelBounds2
        public static PixelBounds2 ReadPixelBounds2(this FormatReaderBase reader, string property, PixelBounds2 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var x = childReader.ReadInt32(nameof(PixelBounds2.X), fallback.X);
            var y = childReader.ReadInt32(nameof(PixelBounds2.Y), fallback.Y);
            var width = childReader.ReadInt32(nameof(PixelBounds2.Width), fallback.Width);
            var height = childReader.ReadInt32(nameof(PixelBounds2.Height), fallback.Height);

            return (new PixelBounds2(x, y, width, height));
        }

        public static void WritePixelBounds2(this FormatWriterBase writer, string property, PixelBounds2 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteInt32(nameof(PixelBounds2.X), value.X);
            childWriter.WriteInt32(nameof(PixelBounds2.Y), value.Y);
            childWriter.WriteInt32(nameof(PixelBounds2.Width), value.Width);
            childWriter.WriteInt32(nameof(PixelBounds2.Height), value.Height);

            writer.Write(property, childWriter);
        }
        #endregion

        #region PixelBounds3
        public static PixelBounds3 ReadPixelBounds3(this FormatReaderBase reader, string property, PixelBounds3 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var x = childReader.ReadInt32(nameof(PixelBounds3.X), fallback.X);
            var y = childReader.ReadInt32(nameof(PixelBounds3.Y), fallback.Y);
            var z = childReader.ReadInt32(nameof(PixelBounds3.Z), fallback.Z);
            var width = childReader.ReadInt32(nameof(PixelBounds3.Width), fallback.Width);
            var height = childReader.ReadInt32(nameof(PixelBounds3.Height), fallback.Height);
            var depth = childReader.ReadInt32(nameof(PixelBounds3.Depth), fallback.Depth);

            return (new PixelBounds3(x, y, z, width, height, depth));
        }

        public static void WritePixelBounds3(this FormatWriterBase writer, string property, PixelBounds3 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteInt32(nameof(PixelBounds3.X), value.X);
            childWriter.WriteInt32(nameof(PixelBounds3.Y), value.Y);
            childWriter.WriteInt32(nameof(PixelBounds3.Z), value.Z);
            childWriter.WriteInt32(nameof(PixelBounds3.Width), value.Width);
            childWriter.WriteInt32(nameof(PixelBounds3.Height), value.Height);
            childWriter.WriteInt32(nameof(PixelBounds3.Depth), value.Depth);

            writer.Write(property, childWriter);
        }
        #endregion

        #region Box2
        public static Box2 ReadBox2(this FormatReaderBase reader, string property, Box2 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var minX = childReader.ReadSingle(nameof(Box2.MinX), fallback.MinX);
            var minY = childReader.ReadSingle(nameof(Box2.MinY), fallback.MinY);
            var maxX = childReader.ReadSingle(nameof(Box2.MaxX), fallback.MaxX);
            var maxY = childReader.ReadSingle(nameof(Box2.MaxY), fallback.MaxY);

            return (new Box2(minX, minY, maxX, maxY));
        }

        public static void WriteBox2(this FormatWriterBase writer, string property, Box2 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteSingle(nameof(Box2.MinX), value.MinX);
            childWriter.WriteSingle(nameof(Box2.MinY), value.MinY);
            childWriter.WriteSingle(nameof(Box2.MaxX), value.MaxX);
            childWriter.WriteSingle(nameof(Box2.MaxY), value.MaxY);

            writer.Write(property, childWriter);
        }
        #endregion

        #region Box3
        public static Box3 ReadBox3(this FormatReaderBase reader, string property, Box3 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var minX = childReader.ReadSingle(nameof(Box3.MinX), fallback.MinX);
            var minY = childReader.ReadSingle(nameof(Box3.MinY), fallback.MinY);
            var minZ = childReader.ReadSingle(nameof(Box3.MinZ), fallback.MinZ);
            var maxX = childReader.ReadSingle(nameof(Box3.MaxX), fallback.MaxX);
            var maxY = childReader.ReadSingle(nameof(Box3.MaxY), fallback.MaxY);
            var maxZ = childReader.ReadSingle(nameof(Box3.MaxZ), fallback.MaxZ);

            return (new Box3(minX, minY, minZ, maxX, maxY, maxZ));
        }

        public static void WriteBox3(this FormatWriterBase writer, string property, Box3 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteSingle(nameof(Box3.MinX), value.MinX);
            childWriter.WriteSingle(nameof(Box3.MinY), value.MinY);
            childWriter.WriteSingle(nameof(Box3.MinZ), value.MinZ);
            childWriter.WriteSingle(nameof(Box3.MaxX), value.MaxX);
            childWriter.WriteSingle(nameof(Box3.MaxY), value.MaxY);
            childWriter.WriteSingle(nameof(Box3.MaxZ), value.MaxZ);

            writer.Write(property, childWriter);
        }
        #endregion

        #region PixelBox2
        public static PixelBox2 ReadPixelBox2(this FormatReaderBase reader, string property, PixelBox2 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var minX = childReader.ReadInt32(nameof(PixelBox2.MinX), fallback.MinX);
            var minY = childReader.ReadInt32(nameof(PixelBox2.MinY), fallback.MinY);
            var maxX = childReader.ReadInt32(nameof(PixelBox2.MaxX), fallback.MaxX);
            var maxY = childReader.ReadInt32(nameof(PixelBox2.MaxY), fallback.MaxY);

            return (new PixelBox2(minX, minY, maxX, maxY));
        }

        public static void WritePixelBox2(this FormatWriterBase writer, string property, PixelBox2 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteInt32(nameof(PixelBox2.MinX), value.MinX);
            childWriter.WriteInt32(nameof(PixelBox2.MinY), value.MinY);
            childWriter.WriteInt32(nameof(PixelBox2.MaxX), value.MaxX);
            childWriter.WriteInt32(nameof(PixelBox2.MaxY), value.MaxY);

            writer.Write(property, childWriter);
        }
        #endregion

        #region PixelBox3
        public static PixelBox3 ReadPixelBox3(this FormatReaderBase reader, string property, PixelBox3 fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var minX = childReader.ReadInt32(nameof(PixelBox3.MinX), fallback.MinX);
            var minY = childReader.ReadInt32(nameof(PixelBox3.MinY), fallback.MinY);
            var minZ = childReader.ReadInt32(nameof(PixelBox3.MinZ), fallback.MinZ);
            var maxX = childReader.ReadInt32(nameof(PixelBox3.MaxX), fallback.MaxX);
            var maxY = childReader.ReadInt32(nameof(PixelBox3.MaxY), fallback.MaxY);
            var maxZ = childReader.ReadInt32(nameof(PixelBox3.MaxZ), fallback.MaxZ);

            return (new PixelBox3(minX, minY, minZ, maxX, maxY, maxZ));
        }

        public static void WritePixelBox3(this FormatWriterBase writer, string property, PixelBox3 value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteInt32(nameof(PixelBox3.MinX), value.MinX);
            childWriter.WriteInt32(nameof(PixelBox3.MinY), value.MinY);
            childWriter.WriteInt32(nameof(PixelBox3.MinZ), value.MinZ);
            childWriter.WriteInt32(nameof(PixelBox3.MaxX), value.MaxX);
            childWriter.WriteInt32(nameof(PixelBox3.MaxY), value.MaxY);
            childWriter.WriteInt32(nameof(PixelBox3.MaxZ), value.MaxZ);

            writer.Write(property, childWriter);
        }
        #endregion

        #region LowTieredScore
        public static LowTieredScore<TValue> ReadLowTieredScore<TValue>(
            this FormatReaderBase reader,
            string property,
            Func<FormatReaderBase, string, TValue> readValueItem,
            LowTieredScore<TValue> fallback = default
        )
             where TValue : struct, IComparable<TValue>
        {
            if (!reader.Contains(property))
                return fallback;

            var readerChild = reader.Read(property);

            if (readerChild == null)
                return fallback;

            var bronze = readValueItem(readerChild, nameof(LowTieredScore<TValue>.Bronze));
            var silver = readValueItem(readerChild, nameof(LowTieredScore<TValue>.Silver));
            var gold = readValueItem(readerChild, nameof(LowTieredScore<TValue>.Gold));

            return new LowTieredScore<TValue>(
                bronze: bronze,
                silver: silver,
                gold: gold
            );
        }

        public static void WriteLowTieredScore<TValue>(
            this FormatWriterBase writer,
            string property,
            LowTieredScore<TValue> value,
            Action<FormatWriterBase, string, TValue?> writeValueItem
        )
             where TValue : struct, IComparable<TValue>
        {
            var writerChild = writer.CreateWriter();

            writeValueItem(writerChild, nameof(LowTieredScore<TValue>.Bronze), value.Bronze);
            writeValueItem(writerChild, nameof(LowTieredScore<TValue>.Silver), value.Silver);
            writeValueItem(writerChild, nameof(LowTieredScore<TValue>.Gold), value.Gold);
        }
        #endregion

        #region TieredScore
        public static HighTieredScore<TValue> ReadHighTieredScore<TValue>(
            this FormatReaderBase reader,
            string property,
            Func<FormatReaderBase, string, TValue> readValueItem,
            HighTieredScore<TValue> fallback = default
        )
             where TValue : struct, IComparable<TValue>
        {
            if (!reader.Contains(property))
                return fallback;

            var readerChild = reader.Read(property);

            if (readerChild == null)
                return fallback;

            var bronze = readValueItem(readerChild, nameof(HighTieredScore<TValue>.Bronze));
            var silver = readValueItem(readerChild, nameof(HighTieredScore<TValue>.Silver));
            var gold = readValueItem(readerChild, nameof(HighTieredScore<TValue>.Gold));

            return new HighTieredScore<TValue>(
                bronze: bronze,
                silver: silver,
                gold: gold
            );
        }

        public static void WriteHighTieredScore<TValue>(
            this FormatWriterBase writer,
            string property,
            HighTieredScore<TValue> value,
            Action<FormatWriterBase, string, TValue?> writeValueItem
        )
             where TValue : struct, IComparable<TValue>
        {
            var writerChild = writer.CreateWriter();

            writeValueItem(writerChild, nameof(HighTieredScore<TValue>.Bronze), value.Bronze);
            writeValueItem(writerChild, nameof(HighTieredScore<TValue>.Silver), value.Silver);
            writeValueItem(writerChild, nameof(HighTieredScore<TValue>.Gold), value.Gold);
        }
        #endregion
    }
}
