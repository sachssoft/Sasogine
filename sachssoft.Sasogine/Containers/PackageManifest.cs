using Sachssoft.Documents;
using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Security.Principal;

namespace Sachssoft.Sasogine.Containers
{
    public class PackageManifest : Document
    {
        private const string FILE_PATH = "manifest";

        internal readonly Dictionary<string, PackageAssetEntry> _assets = new(StringComparer.InvariantCultureIgnoreCase);
        internal readonly ObservableCollection<PackageLevelBase> _levels = new();
        internal PackageBase _package;

        public PackageManifest()
        {

        }

        public void Load()
        {
            _package.ThrowIfNotOpened();
            _package.ThrowIfIsReadOnly();

            var entry = _package.Source.GetEntry(FILE_PATH);

            if (entry == null)
                return;

            using var stream = entry.Open();
            Load(stream, _package.ManifestFormat);
        }

        protected override void OnRead(FormatReaderBase reader)
        {
            ReadAssets(reader);
            ReadLevels(reader);
        }

        public void Save()
        {
            _package.ThrowIfNotOpened();
            _package.ThrowIfIsReadOnly();

            var entry = _package.Source.GetEntry(FILE_PATH)
                ?? _package.Source.CreateEntry(FILE_PATH);

            using var stream = entry.Open();
            stream.SetLength(0); // Inhalt löschen, falls vorhanden
            Save(stream, _package.ManifestFormat);
        }

        protected override void OnWritten(FormatWriterBase writer)
        {
            WriteAssets(writer);
            WriteLevels(writer);
        }

        private void ReadAssets(FormatReaderBase reader)
        {
            if (!reader.Contains("$assets"))
                return;

            var assetReaders = new List<FormatReaderBase>(reader.ReadArray("$assets"));
            var unknownIndex = 0;

            foreach (var readerItem in assetReaders)
            {
                var entryReader = (JsonReader)readerItem;
                var entry = new PackageAssetEntry(_package);

                entry.Guid = entryReader.ReadGuid(GetNamingValue(nameof(PackageAssetEntry.Guid)), Guid.NewGuid());
                entry.FileName = entryReader.ReadString(GetNamingValue(nameof(PackageAssetEntry.FileName))) ?? $"$unknown{unknownIndex++}";
                entry.Category = entryReader.ReadEnum<AssetCategory>(GetNamingValue(nameof(PackageAssetEntry.Category)));
                entry.CategoryName = entryReader.ReadString(GetNamingValue(nameof(PackageAssetEntry.CategoryName))) ?? "";
                entry.Hash = entryReader.ReadString(GetNamingValue(nameof(PackageAssetEntry.Hash)));

                _assets.Add(entry.FileName, entry);
            }
        }

        private void ReadLevels(FormatReaderBase reader)
        {
        }

        private void WriteAssets(FormatWriterBase writer)
        {
            var assetWriters = new List<FormatWriterBase>();
            foreach (var asset in _assets)
            {
                var entryWriter = new JsonWriter();
                entryWriter.WriteString(GetNamingValue(nameof(PackageAssetEntry.FileName)), asset.Value.FileName);
                entryWriter.WriteEnum(GetNamingValue(nameof(PackageAssetEntry.Category)), asset.Value.Category);
                entryWriter.WriteString(GetNamingValue(nameof(PackageAssetEntry.CategoryName)), asset.Value.CategoryName);
                entryWriter.WriteString(GetNamingValue(nameof(PackageAssetEntry.Hash)), asset.Value.Hash);
                entryWriter.WriteGuid(GetNamingValue(nameof(PackageAssetEntry.Guid)), asset.Value.Guid);

                assetWriters.Add(entryWriter);
            }
            writer.WriteArray("$assets", assetWriters.ToArray());
        }

        private void WriteLevels(FormatWriterBase writer)
        {

        }

        public bool TryGetAsset(string filePath, out PackageAssetEntry? asset)
        {
            return _assets.TryGetValue(filePath, out asset);
        }
    }
}
