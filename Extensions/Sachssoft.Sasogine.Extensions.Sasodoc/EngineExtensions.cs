using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Gameplay;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
    public static class EngineExtensions
    {
        #region Size
        public static Size ReadSize(this FormatReaderBase reader, string property, Size fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var width = childReader.ReadSingle(nameof(Size.Width), fallback.Width);
            var height = childReader.ReadSingle(nameof(Size.Height), fallback.Height);

            return (new Size(width, height));
        }

        public static void WriteSize(this FormatWriterBase writer, string property, Size value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteSingle(nameof(Size.Width), value.Width);
            childWriter.WriteSingle(nameof(Size.Height), value.Height);

            writer.Write(property, childWriter);
        }
        #endregion

        #region PixelSize
        public static PixelSize ReadPixelSize(this FormatReaderBase reader, string property, PixelSize fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var width = childReader.ReadInt32(nameof(PixelSize.Width), fallback.Width);
            var height = childReader.ReadInt32(nameof(PixelSize.Height), fallback.Height);

            return (new PixelSize(width, height));
        }

        public static void WritePixelSize(this FormatWriterBase writer, string property, PixelSize value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteInt32(nameof(PixelSize.Width), value.Width);
            childWriter.WriteInt32(nameof(PixelSize.Height), value.Height);

            writer.Write(property, childWriter);
        }
        #endregion

        #region Insets
        public static Insets ReadInsets(this FormatReaderBase reader, string property, Insets fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var left = childReader.ReadSingle(nameof(Insets.Left), fallback.Left);
            var top = childReader.ReadSingle(nameof(Insets.Top), fallback.Top);
            var right = childReader.ReadSingle(nameof(Insets.Right), fallback.Right);
            var bottom = childReader.ReadSingle(nameof(Insets.Bottom), fallback.Bottom);

            return (new Insets(left, top, right, top));
        }

        public static void WriteInsets(this FormatWriterBase writer, string property, Insets value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteSingle(nameof(Insets.Left), value.Left);
            childWriter.WriteSingle(nameof(Insets.Top), value.Top);
            childWriter.WriteSingle(nameof(Insets.Right), value.Right);
            childWriter.WriteSingle(nameof(Insets.Bottom), value.Bottom);

            writer.Write(property, childWriter);
        }
        #endregion

        #region PixelInsets
        public static PixelInsets ReadPixelInsets(this FormatReaderBase reader, string property, PixelInsets fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var left = childReader.ReadInt32(nameof(PixelInsets.Left), fallback.Left);
            var top = childReader.ReadInt32(nameof(PixelInsets.Top), fallback.Top);
            var right = childReader.ReadInt32(nameof(PixelInsets.Right), fallback.Right);
            var bottom = childReader.ReadInt32(nameof(PixelInsets.Bottom), fallback.Bottom);

            return (new PixelInsets(left, top, right, top));
        }

        public static void WritePixelInsets(this FormatWriterBase writer, string property, PixelInsets value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteInt32(nameof(PixelInsets.Left), value.Left);
            childWriter.WriteInt32(nameof(PixelInsets.Top), value.Top);
            childWriter.WriteInt32(nameof(PixelInsets.Right), value.Right);
            childWriter.WriteInt32(nameof(PixelInsets.Bottom), value.Bottom);

            writer.Write(property, childWriter);
        }
        #endregion

        #region Bounds
        public static Bounds ReadBounds(this FormatReaderBase reader, string property, Bounds fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var x = childReader.ReadSingle(nameof(Bounds.X), fallback.X);
            var y = childReader.ReadSingle(nameof(Bounds.Y), fallback.Y);
            var width = childReader.ReadSingle(nameof(Bounds.Width), fallback.Width);
            var height = childReader.ReadSingle(nameof(Bounds.Height), fallback.Height);

            return (new Bounds(x, y, width, y));
        }

        public static void WriteBounds(this FormatWriterBase writer, string property, Bounds value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteSingle(nameof(Bounds.X), value.X);
            childWriter.WriteSingle(nameof(Bounds.Y), value.Y);
            childWriter.WriteSingle(nameof(Bounds.Width), value.Width);
            childWriter.WriteSingle(nameof(Bounds.Height), value.Height);

            writer.Write(property, childWriter);
        }
        #endregion

        #region PixelBounds
        public static PixelBounds ReadPixelBounds(this FormatReaderBase reader, string property, PixelBounds fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var x = childReader.ReadInt32(nameof(PixelBounds.X), fallback.X);
            var y = childReader.ReadInt32(nameof(PixelBounds.Y), fallback.Y);
            var width = childReader.ReadInt32(nameof(PixelBounds.Width), fallback.Width);
            var height = childReader.ReadInt32(nameof(PixelBounds.Height), fallback.Height);

            return (new PixelBounds(x, y, width, y));
        }

        public static void WritePixelBounds(this FormatWriterBase writer, string property, PixelBounds value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteInt32(nameof(PixelBounds.X), value.X);
            childWriter.WriteInt32(nameof(PixelBounds.Y), value.Y);
            childWriter.WriteInt32(nameof(PixelBounds.Width), value.Width);
            childWriter.WriteInt32(nameof(PixelBounds.Height), value.Height);

            writer.Write(property, childWriter);
        }
        #endregion

        #region Box
        public static Box ReadBox(this FormatReaderBase reader, string property, Box fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var minX = childReader.ReadSingle(nameof(Box.MinX), fallback.MinX);
            var minY = childReader.ReadSingle(nameof(Box.MinY), fallback.MinY);
            var maxX = childReader.ReadSingle(nameof(Box.MaxX), fallback.MaxX);
            var maxY = childReader.ReadSingle(nameof(Box.MaxY), fallback.MaxY);

            return (new Box(minX, minY, maxX, minY));
        }

        public static void WriteBox(this FormatWriterBase writer, string property, Box value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteSingle(nameof(Box.MinX), value.MinX);
            childWriter.WriteSingle(nameof(Box.MinY), value.MinY);
            childWriter.WriteSingle(nameof(Box.MaxX), value.MaxX);
            childWriter.WriteSingle(nameof(Box.MaxY), value.MaxY);

            writer.Write(property, childWriter);
        }
        #endregion

        #region PixelBox
        public static PixelBox ReadPixelBox(this FormatReaderBase reader, string property, PixelBox fallback)
        {
            var childReader = reader.Read(property);

            if (childReader == null)
                return fallback;

            var minX = childReader.ReadInt32(nameof(PixelBox.MinX), fallback.MinX);
            var minY = childReader.ReadInt32(nameof(PixelBox.MinY), fallback.MinY);
            var maxX = childReader.ReadInt32(nameof(PixelBox.MaxX), fallback.MaxX);
            var maxY = childReader.ReadInt32(nameof(PixelBox.MaxY), fallback.MaxY);

            return (new PixelBox(minX, minY, maxX, minY));
        }

        public static void WritePixelBox(this FormatWriterBase writer, string property, PixelBox value)
        {
            var childWriter = writer.CreateWriter();

            childWriter.WriteInt32(nameof(PixelBox.MinX), value.MinX);
            childWriter.WriteInt32(nameof(PixelBox.MinY), value.MinY);
            childWriter.WriteInt32(nameof(PixelBox.MaxX), value.MaxX);
            childWriter.WriteInt32(nameof(PixelBox.MaxY), value.MaxY);

            writer.Write(property, childWriter);
        }
        #endregion

        #region TieredScore
        public static TieredScore<TValue> ReadTieredScore<TValue>(
            this FormatReaderBase reader,
            string property,
            Func<FormatReaderBase, string, TValue> readValueItem,
            ScoreDirection direction,
            TieredScore<TValue> fallback = default
        )
             where TValue : struct, IComparable<TValue>
        {
            if (!reader.Contains(property))
                return fallback;

            var readerChild = reader.Read(property);

            if (readerChild == null)
                return fallback;

            var bronze = readValueItem(readerChild, nameof(TieredScore<TValue>.Bronze));
            var silver = readValueItem(readerChild, nameof(TieredScore<TValue>.Silver));
            var gold = readValueItem(readerChild, nameof(TieredScore<TValue>.Gold));

            return new TieredScore<TValue>(
                bronze: bronze,
                silver: silver,
                gold: gold,
                direction: direction
            );
        }

        public static void WriteTieredScore<TValue>(
            this FormatWriterBase writer,
            string property,
            TieredScore<TValue> value,
            Action<FormatWriterBase, string, TValue?> writeValueItem
        )
             where TValue : struct, IComparable<TValue>
        {
            var writerChild = writer.CreateWriter();

            writeValueItem(writerChild, nameof(TieredScore<TValue>.Bronze), value.Bronze);
            writeValueItem(writerChild, nameof(TieredScore<TValue>.Silver), value.Silver);
            writeValueItem(writerChild, nameof(TieredScore<TValue>.Gold), value.Gold);
        }
        #endregion
    }
}
