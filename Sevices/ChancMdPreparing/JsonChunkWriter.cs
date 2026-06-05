using System;
using System.Collections.Generic;
using System.Text;

namespace LlamaParserV2.Sevices.ChancMdPreparing;

using System.Text.Encodings.Web;
using System.Text.Json;

public sealed class JsonChunkWriter
{
    public async Task WriteAsync(
        string outputPath,
        IReadOnlyCollection<RegulationChunk> chunks)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var json = JsonSerializer.Serialize(chunks, options);

        await File.WriteAllTextAsync(outputPath, json);
    }
}