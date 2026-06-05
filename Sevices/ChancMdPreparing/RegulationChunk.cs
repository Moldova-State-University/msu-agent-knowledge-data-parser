using System;
using System.Collections.Generic;
using System.Text;

namespace LlamaParserV2.Sevices.ChancMdPreparing;

public sealed class RegulationChunk
{
    public required string Id { get; init; }
    public required string DocumentId { get; init; }
    public required string DocumentTitle { get; init; }

    public required int ChunkIndex { get; init; }

    public required List<string> Headings { get; init; }
    public required string HeadingPath { get; init; }
    public required int HeadingLevel { get; init; }

    public required string ChunkText { get; init; }
    public required string EmbeddingText { get; init; }

    public required string Language { get; init; }
    public string? Version { get; init; }
    public string? ValidFrom { get; init; }

    public required string SourceFile { get; init; }
    public required string Hash { get; init; }
}