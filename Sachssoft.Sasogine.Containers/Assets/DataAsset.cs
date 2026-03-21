using Sachssoft.Sasofly.Inspection;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Represents a text asset that can be loaded from an <see cref="IAssetSource"/>.
    /// </summary>
    public class DataAsset : AssetBase<string>, ITypeRegistry
    {
        static void ITypeRegistry.RegisterProperties(TypeRegistryContext context)
        {
            context.RegisterProperty(TargetProperty);
            context.RegisterProperty(FormatProperty);
            context.RegisterProperty(EncodingProperty);
        }

        /// <summary>
        /// Builds the string content from the specified stream.
        /// </summary>
        /// <param name="stream">The stream containing text data.</param>
        /// <returns>The loaded text, or <c>null</c> if loading fails.</returns>
        protected override string? Build(Stream stream)
        {
            if (stream == null || stream.Length == 0)
                return null;

            try
            {
                using var reader = new StreamReader(stream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Exception = ex;
                return null;
            }
        }

        public static readonly IProperty TargetProperty =
            new StoredProperty<DataAsset, DataAssetTarget>(
                nameof(Target),
                defaultValue: DataAssetTarget.Custom,
                category: PropertyCategories.General,
                metadata: new PropertySerializationMetadata(
                    deserialize: (p, r) => r.ReadEnum<DataAssetTarget>(context: p.Name),
                    serialize: (p, w, v) => w.WriteEnum<DataAssetTarget>(context: p.Name, (DataAssetTarget)(v ?? DataAssetTarget.Custom)))
                {
                    Visibility = PropertyVisibility.Visible
                });

        public DataAssetTarget Target
        {
            get => GetValue<DataAssetTarget>(TargetProperty);
            set => SetValue<DataAssetTarget>(TargetProperty, value);
        }

        public static readonly IProperty FormatProperty =
            new StoredProperty<DataAsset, DataAssetFormat>(
                nameof(Format),
                defaultValue: DataAssetFormat.Text,
                category: PropertyCategories.General,
                metadata: new PropertySerializationMetadata(
                    deserialize: (p, r) => r.ReadEnum<DataAssetFormat>(context: p.Name),
                    serialize: (p, w, v) => w.WriteEnum<DataAssetFormat>(context: p.Name, (DataAssetFormat)(v ?? DataAssetFormat.Text)))
                {
                    Visibility = PropertyVisibility.Visible
                });

        public DataAssetFormat Format
        {
            get => GetValue<DataAssetFormat>(FormatProperty);
            set => SetValue<DataAssetFormat>(FormatProperty, value);
        }

        public static readonly IProperty EncodingProperty =
            new StoredProperty<DataAsset, DataAssetEncoding>(
                nameof(Encoding),
                defaultValue: DataAssetEncoding.UTF8,
                category: PropertyCategories.General,
                metadata: new PropertySerializationMetadata(
                    deserialize: (p, r) => r.ReadEnum<DataAssetEncoding>(context: p.Name),
                    serialize: (p, w, v) => w.WriteEnum<DataAssetEncoding>(context: p.Name, (DataAssetEncoding)(v ?? DataAssetEncoding.UTF8)))
                {
                    Visibility = PropertyVisibility.Visible
                });

        public DataAssetEncoding Encoding
        {
            get => GetValue<DataAssetEncoding>(EncodingProperty);
            set => SetValue<DataAssetEncoding>(EncodingProperty, value);
        }
    }
}
