using Sachssoft.Documents;
using Sachssoft.Documents.Json;
using Sachssoft.Sasogine.Assets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;

namespace Sachssoft.Sasogine.Containers
{
    public class PackageManifest : Document
    {
        private const string FILE_PATH = "manifest";
        private const string SECTION_ASSETS = "$assets";
        private const string SECTION_LEVELS = "$levels";

        internal readonly Dictionary<string, PackageAssetEntry> _assets = new(StringComparer.InvariantCultureIgnoreCase);
        internal readonly ObservableCollection<PackageLevelBase> _levels = new();
        internal PackageBase _package;

        protected IPackageAssetFactory? AssetFactory { get; set; }

        protected IPackageLevelFactory LevelFactory { get; set; }

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
            //if (AssetFactory == null)
            //    throw new InvalidOperationException(
            //        $"{nameof(AssetFactory)} is not set. You must set it before creating level instances.");

            _assets.Clear();

            if (!reader.Contains(SECTION_ASSETS))
                return;

            var assetReaders = new List<FormatReaderBase>(reader.ReadArray(SECTION_ASSETS));
            var unknownIndex = 0;

            foreach (var readerItem in assetReaders)
            {
                var entryReader = (JsonReader)readerItem;
                //var entry = AssetFactory.Invoke(_package);
                var entry = new PackageAssetEntry(_package);

                entry.Guid = entryReader.ReadGuid(GetNamingValue(nameof(PackageAssetEntry.Guid)), Guid.NewGuid());
                entry.FileName = entryReader.ReadString(GetNamingValue(nameof(PackageAssetEntry.FileName))) ?? $"$unknown{unknownIndex++}";
                entry.Category = entryReader.ReadEnum<AssetCategory>(GetNamingValue(nameof(PackageAssetEntry.Category)));
                entry.CategoryName = entryReader.ReadString(GetNamingValue(nameof(PackageAssetEntry.CategoryName))) ?? "";
                entry.Hash = entryReader.ReadString(GetNamingValue(nameof(PackageAssetEntry.Hash)));
                entry.TypeFactoryKey = entryReader.ReadString(GetNamingValue(nameof(PackageAssetEntry.TypeFactoryKey)));

                if (AssetFactory != null)
                {
                    entry.Asset = AssetFactory.Build(_package, entry);
                }

                _assets.Add(entry.FileName, entry);
            }
        }

        private void ReadLevels(FormatReaderBase reader)
        {
            if (LevelFactory == null)
                throw new InvalidOperationException(
                    $"{nameof(LevelFactory)} is not set. You must set it before creating level instances.");

            _levels.Clear();

            if (!reader.Contains(SECTION_LEVELS))
                return;

            var sectionReader = (JsonReader)reader.Read(SECTION_LEVELS)!;

            if (_package is ProjectedPackage projected)
            {
                projected.SelectedLevelIndex = sectionReader.ReadInt32("selected-index");
            }

            var entries = new List<FormatReaderBase>(sectionReader.ReadArray("entries"));
            var unsortedLevelList = new List<PackageLevelBase>();

            foreach (var readerItem in entries)
            {
                var entryReader = (JsonReader)readerItem;
                var filePath = entryReader.ReadString(GetNamingValue(nameof(PackageLevelBase.RelativeFilePath))) ?? "";

                var levelEntry = LevelFactory.Build(_package, filePath) ??
                    throw new InvalidOperationException("Can not create level instance");

                levelEntry.ID = entryReader.ReadString(GetNamingValue(nameof(PackageLevelBase.ID)));
                levelEntry.Name = entryReader.ReadString(GetNamingValue(nameof(PackageLevelBase.Name)));
                levelEntry.Class = entryReader.ReadString(GetNamingValue(nameof(PackageLevelBase.Class)));
                levelEntry.Index = entryReader.ReadInt32(GetNamingValue(nameof(PackageLevelBase.Index)));
                levelEntry.Guid = entryReader.ReadGuid(GetNamingValue(nameof(PackageLevelBase.Guid)), Guid.NewGuid());
                levelEntry.Title = entryReader.ReadString(GetNamingValue(nameof(PackageLevelBase.Title)));
                levelEntry.Description = entryReader.ReadString(GetNamingValue(nameof(PackageLevelBase.Description)));
                levelEntry.RelativeFilePath = filePath;

                unsortedLevelList.Add(levelEntry);
            }

            var sortedLevelList = unsortedLevelList.OrderBy(x => x.Index)
                                                   .ToArray();
            for(int i = 0;  i < sortedLevelList.Length; i++)
            {
                var levelEntry = sortedLevelList[i];
                levelEntry.Index = i;
                _levels.Add(levelEntry);
            }
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
                entryWriter.WriteString(GetNamingValue(nameof(PackageAssetEntry.TypeFactoryKey)), asset.Value.TypeFactoryKey);

                assetWriters.Add(entryWriter);
            }
            writer.WriteArray(SECTION_ASSETS, assetWriters.ToArray());
        }

        private void WriteLevels(FormatWriterBase writer)
        {
            if (_package is not ProjectedPackage projected)
                throw new InvalidOperationException("Package is read-only");

            var levelWriters = new List<FormatWriterBase>();
            foreach (var level in _levels)
            {
                var entryWriter = new JsonWriter();

                entryWriter.WriteString(GetNamingValue(nameof(PackageLevelBase.ID)), level.ID);
                entryWriter.WriteString(GetNamingValue(nameof(PackageLevelBase.Name)), level.Name);
                entryWriter.WriteString(GetNamingValue(nameof(PackageLevelBase.Class)), level.Class);
                entryWriter.WriteInt32(GetNamingValue(nameof(PackageLevelBase.Index)), level.Index);
                entryWriter.WriteGuid(GetNamingValue(nameof(PackageLevelBase.Guid)), level.Guid);
                entryWriter.WriteString(GetNamingValue(nameof(PackageLevelBase.RelativeFilePath)), level.RelativeFilePath);
                entryWriter.WriteString(GetNamingValue(nameof(PackageLevelBase.Title)), level.Title);
                entryWriter.WriteString(GetNamingValue(nameof(PackageLevelBase.Description)), level.Description);

                if (level.IsDirty && level.Index == projected.SelectedLevelIndex)
                {
                    level.Save();
                }

                levelWriters.Add(entryWriter);
            }

            var sectionWriter = new JsonWriter();
            sectionWriter.WriteInt32("selected-index", projected.SelectedLevelIndex);
            sectionWriter.WriteArray("entries", levelWriters.ToArray());

            writer.Write(SECTION_LEVELS, sectionWriter);
        }

        public bool TryGetAsset(string filePath, out PackageAssetEntry? asset)
        {
            return _assets.TryGetValue(filePath, out asset);
        }
    }
}
