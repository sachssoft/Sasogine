using Sachssoft.Documents;
using Sachssoft.Documents.Json;
using Sachssoft.Naming;
using Sachssoft.Observables;
using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sachssoft.Sasogine.Containers
{
    public class PackageManifest : Document
    {
        private const string FILE_PATH = "manifest";
        private readonly string _sectionAssets;
        private readonly string _sectionLevels;
        private readonly string _sectionData;

        internal readonly Dictionary<string, IAssetSource> _assetEntries = new(StringComparer.InvariantCultureIgnoreCase);
        internal readonly ObservableCollection<PackageLevelBase> _levels = new();
        [AllowNull] internal PackageBase _package;

        public PackageManifest()
        {
            _sectionAssets = "$" + "assets".ToCase(Options.PropertyNamingConvention, Options.PropertyNamingOptions);
            _sectionLevels = "$" + "levels".ToCase(Options.PropertyNamingConvention, Options.PropertyNamingOptions);
            _sectionData = "$" + "data".ToCase(Options.PropertyNamingConvention, Options.PropertyNamingOptions);

            Options.PreservedPropertyNames = [
                _sectionAssets,
                _sectionLevels,
                _sectionData,
            ];
        }

        protected IPackageAssetFactory? AssetFactory { get; set; }

        protected IPackageLevelFactory? LevelFactory { get; set; }

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

        [System.Diagnostics.Conditional("DEBUG")]
        private void HintIfNoFactory(object? obj, string factoryName)
        {
            if (obj == null)
            {
                var msg =
                    $"Hint: The factory '{factoryName}' is null. " +
                    "Without a factory, no entries can be read from the file, and no instances can be created. ";

                System.Diagnostics.Debug.WriteLine(msg);
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break(); // Stoppt nur, wenn Debugger läuft
            }
        }

        private void ReadAssets(FormatReaderBase reader)
        {
            if (AssetFactory == null)
            {
                HintIfNoFactory(AssetFactory, nameof(AssetFactory));
                return;
            }

            _assetEntries.Clear();

            if (!reader.Contains(_sectionAssets))
                return;

            var assetReaders = new List<FormatReaderBase>(reader.ReadArray(_sectionAssets));
            var unknownIndex = 0;
            var _assetIdCounter = 0;

            foreach (var readerItem in assetReaders)
            {
                var entryReader = (JsonReader)readerItem;
                entryReader.Options = Options;

                var entry = new PackageAssetEntry(_package);

                entry.Guid = entryReader.ReadGuid(nameof(PackageAssetEntry.Guid), Guid.NewGuid());
                entry.FileName = entryReader.ReadString(nameof(PackageAssetEntry.FileName)) ?? $"$unknown{unknownIndex++}";
                entry.Category = entryReader.ReadEnum<AssetCategory>(nameof(PackageAssetEntry.Category));
                entry.CategoryName = entryReader.ReadString(nameof(PackageAssetEntry.CategoryName)) ?? "";
                entry.Hash = entryReader.ReadString(nameof(PackageAssetEntry.Hash));
                entry.TypeName = entryReader.ReadString(nameof(PackageAssetEntry.TypeName));
                entry.Asset = AssetFactory.Build(_package, entry);

                if (entry.Asset is NotifyingObject nAsset)
                {
                    var assetReader = entryReader.Read(_sectionData);
                    assetReader.Options = Options;
                    assetReader.Deserialize(nAsset);
                }

                if (entry.Asset != null)
                {
                    // ID setzen, falls leer
                    if (string.IsNullOrEmpty(entry.Asset.ID))
                    {
                        if (entry.Asset.Source != null && !string.IsNullOrEmpty(entry.Asset.Source.FileName))
                        {
                            entry.Asset.ID = entry.Asset.Source.FileName;
                        }
                        else
                        {
                            entry.Asset.ID = $"asset{_assetIdCounter++}";
                        }
                    }
                }

                _assetEntries.Add(entry.FileName, entry);
            }
        }

        private void ReadLevels(FormatReaderBase reader)
        {
            if (LevelFactory == null)
            {
                HintIfNoFactory(LevelFactory, nameof(LevelFactory));
                return;
            }

            _levels.Clear();

            if (!reader.Contains(_sectionLevels))
                return;

            var sectionReader = (JsonReader)reader.Read(_sectionLevels)!;

            if (_package is ProjectedPackage projected)
            {
                projected.SelectedLevelIndex = sectionReader.ReadInt32("selected-index");
            }

            var entries = new List<FormatReaderBase>(sectionReader.ReadArray("entries"));
            var unsortedLevelList = new List<PackageLevelBase>();

            foreach (var readerItem in entries)
            {
                var entryReader = (JsonReader)readerItem;
                entryReader.Options = Options;
                //entryReader.NamingConvention = NamingConvention;
                //entryReader.NamingOptions = NamingOptions;

                var filePath = entryReader.ReadString((nameof(PackageLevelBase.RelativeFilePath))) ?? "";

                var levelEntry = LevelFactory.Build(_package, filePath) ??
                    throw new InvalidOperationException("Can not create level instance");

                levelEntry.ID = entryReader.ReadString((nameof(PackageLevelBase.ID)));
                levelEntry.Name = entryReader.ReadString((nameof(PackageLevelBase.Name)));
                levelEntry.Class = entryReader.ReadString((nameof(PackageLevelBase.Class)));
                levelEntry.Index = entryReader.ReadInt32((nameof(PackageLevelBase.Index)));
                levelEntry.Guid = entryReader.ReadGuid((nameof(PackageLevelBase.Guid)), Guid.NewGuid());
                levelEntry.Title = entryReader.ReadString((nameof(PackageLevelBase.Title)));
                levelEntry.Description = entryReader.ReadString((nameof(PackageLevelBase.Description)));
                levelEntry.RelativeFilePath = filePath;

                unsortedLevelList.Add(levelEntry);
            }

            var sortedLevelList = unsortedLevelList.OrderBy(x => x.Index)
                                                   .ToArray();
            for (int i = 0; i < sortedLevelList.Length; i++)
            {
                var levelEntry = sortedLevelList[i];
                levelEntry.Index = i;
                _levels.Add(levelEntry);
            }
        }

        private void WriteAssets(FormatWriterBase writer)
        {
            var assetWriters = new List<FormatWriterBase>();
            foreach (var assetEntry in _assetEntries)
            {
                var entryWriter = new JsonWriter();
                //entryWriter.NamingConvention = NamingConvention;
                //entryWriter.NamingOptions = NamingOptions;
                entryWriter.Options = Options;

                var entry = (PackageAssetEntry)assetEntry.Value;

                //var asset = entry.Build(_package, filePath) ??
                //    throw new InvalidOperationException("Can not create level instance");

                entryWriter.WriteString(nameof(PackageAssetEntry.FileName), entry.FileName);
                entryWriter.WriteEnum(nameof(PackageAssetEntry.Category), entry.Category);
                entryWriter.WriteString(nameof(PackageAssetEntry.CategoryName), entry.CategoryName);
                entryWriter.WriteString(nameof(PackageAssetEntry.Hash), entry.Hash);
                entryWriter.WriteGuid(nameof(PackageAssetEntry.Guid), entry.Guid);
                entryWriter.WriteString(nameof(PackageAssetEntry.TypeName), entry.TypeName);

                if (entry.Asset is NotifyingObject nAsset)
                {
                    var assetWriter = new JsonWriter();
                    assetWriter.Options = Options;
                    //Serialize(nAsset, assetWriter);
                    assetWriter.Serialize(nAsset);
                    entryWriter.Write(_sectionData, assetWriter);
                }

                assetWriters.Add(entryWriter);
            }
            writer.WriteArray(_sectionAssets, assetWriters.ToArray());
        }

        private void WriteLevels(FormatWriterBase writer)
        {
            if (_package is not ProjectedPackage projected)
                throw new InvalidOperationException("Package is read-only");

            var levelWriters = new List<FormatWriterBase>();
            foreach (var level in _levels)
            {
                var entryWriter = new JsonWriter();
                //entryWriter.NamingConvention = NamingConvention;
                //entryWriter.NamingOptions = NamingOptions;
                entryWriter.Options = Options;

                entryWriter.WriteString((nameof(PackageLevelBase.ID)), level.ID);
                entryWriter.WriteString((nameof(PackageLevelBase.Name)), level.Name);
                entryWriter.WriteString((nameof(PackageLevelBase.Class)), level.Class);
                entryWriter.WriteInt32((nameof(PackageLevelBase.Index)), level.Index);
                entryWriter.WriteGuid((nameof(PackageLevelBase.Guid)), level.Guid);
                entryWriter.WriteString((nameof(PackageLevelBase.RelativeFilePath)), level.RelativeFilePath);
                entryWriter.WriteString((nameof(PackageLevelBase.Title)), level.Title);
                entryWriter.WriteString((nameof(PackageLevelBase.Description)), level.Description);

                if (level.IsDirty && level.Index == projected.SelectedLevelIndex)
                {
                    level.Save();
                }

                levelWriters.Add(entryWriter);
            }

            var sectionWriter = new JsonWriter();
            sectionWriter.WriteInt32("selected-index", projected.SelectedLevelIndex);
            sectionWriter.WriteArray("entries", levelWriters.ToArray());

            writer.Write(_sectionLevels, sectionWriter);
        }

        public bool TryGetAsset(string filePath, [MaybeNullWhen(false)] out PackageAssetEntry? asset)
        {
            var result = _assetEntries.TryGetValue(filePath, out var entry);
            asset = (PackageAssetEntry?)entry;
            return result;
        }
    }
}
