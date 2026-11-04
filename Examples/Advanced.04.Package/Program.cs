using System.Text;
using Sachssoft.Engine.Packages;

var packagePath = Path.Combine(AppContext.BaseDirectory, "sample.example");
var version = new Version(1, 2, 0);
byte[] metadata = "Name: Container example\nAuthor: Sasogine"u8.ToArray();

using var levelBuffer = new MemoryStream();
using (var levelWriter = new BinaryWriter(levelBuffer, Encoding.UTF8, leaveOpen: true))
{
    levelWriter.Write(128);
    for (int index = 0; index < 128; index++)
        levelWriter.Write(42);
}

var entries = new Dictionary<string, ReadOnlyMemory<byte>>
{
    ["levels/level-01.bin"] = levelBuffer.ToArray(),
    ["assets/note.txt"] = "Sample asset"u8.ToArray()
};

using (var destination = File.Create(packagePath))
    PackageContainer.Write(destination, "EXMP"u8, version, metadata, entries);

using var source = File.OpenRead(packagePath);
using (var package = PackageContainer.Open(source, "EXMP"u8, leaveOpen: true))
{
    using var metadataStream = package.OpenMetadata();
    using var metadataReader = new StreamReader(metadataStream, Encoding.UTF8);
    string metadataText = metadataReader.ReadToEnd();

    using var level = package.OpenEntry("levels/level-01.bin");
    using var levelReader = new BinaryReader(level);
    int valueCount = levelReader.ReadInt32();
    if (package.Version != version || metadataText != Encoding.UTF8.GetString(metadata) || valueCount != 128)
        throw new InvalidDataException("The package did not round-trip.");

    for (int index = 0; index < valueCount; index++)
    {
        if (levelReader.ReadInt32() != 42)
            throw new InvalidDataException("The binary level data did not round-trip.");
    }

    Console.WriteLine($"Version: {package.Version}");
    Console.WriteLine(metadataText);
    Console.WriteLine($"Binary level: {valueCount} values");
}

source.Position = 0;
using var inspected = PackageContainer.Open(source, leaveOpen: true);
using var asset = inspected.OpenEntry("assets/note.txt");
using var assetReader = new StreamReader(asset, Encoding.UTF8);
if (assetReader.ReadToEnd() != "Sample asset")
    throw new InvalidDataException("The asset did not round-trip.");

Console.WriteLine($"Verified {inspected.EntryPaths.Count()} entries with both Open overloads.");
Console.WriteLine($"Package: {packagePath} ({source.Length} bytes)");
