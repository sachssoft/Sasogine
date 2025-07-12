using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System;
using System.ComponentModel;
using System.IO;

namespace tcs_GameEnginePipeline;

[ContentImporter(".json", DefaultProcessor = nameof(JsonContentProcessor), DisplayName = "JSON Importer - MonoGame.Extended")]
public class JsonContentImporter : ContentImporter<JsonContentImporterResult>
{
    public override JsonContentImporterResult Import(string filename, ContentImporterContext context)
    {
        var json = File.ReadAllText(filename);
        return new JsonContentImporterResult(filename, json);
    }
}

[ContentProcessor(DisplayName = "JSON Processor - MonoGame.Extended")]
public class JsonContentProcessor : ContentProcessor<JsonContentImporterResult, JsonContentProcessorResult>
{
    [DefaultValue(typeof(Type), "System.Object")]
    public string ContentType { get; set; }

    public override JsonContentProcessorResult Process(JsonContentImporterResult input, ContentProcessorContext context)
    {
        try
        {
            var output = new JsonContentProcessorResult
            {
                ContentType = ContentType,
                Json = input.Data
            };
            return output;
        }
        catch (Exception ex)
        {
            context.Logger.LogMessage("Error {0}", ex);
            throw;
        }
    }
}

public class JsonContentImporterResult
{
    public JsonContentImporterResult(string filePath, string data)
    {
        FilePath = filePath;
        Data = data;
    }

    public string FilePath { get; }
    public string Data { get; }
}

public class JsonContentProcessorResult
{
    public string ContentType { get; set; }
    public string Json { get; set; }
}

[ContentTypeWriter]
public class JsonContentTypeWriter : ContentTypeWriter<JsonContentProcessorResult>
{
    private string _runtimeType;

    protected override void Write(ContentWriter writer, JsonContentProcessorResult result)
    {
        _runtimeType = result.ContentType;
        writer.Write(result.Json);
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return _runtimeType;
    }

    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
        return _runtimeType;
    }
}
