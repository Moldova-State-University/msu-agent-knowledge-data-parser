using System;
using System.Collections.Generic;
using System.IO;

namespace LlamaParserV2.Sevices.ChancMdPreparing;

public sealed class MarkdownChunkParser
{
    private const string ChunkEndMarker = "[chunk_end]";

    public List<RegulationChunk> Parse(
        string markdown,
        string documentId,
        string sourceFile,
        string language = "ro")
    {
        var chunks = new List<RegulationChunk>();
        var headings = new List<string>();
        var documentTitle = documentId;
        var chunkIndex = 0;
        var buffer = new List<string>();
        List<string>? chunkHeadings = null;
        string? chunkDocumentTitle = null;

        var normalizedMarkdown = markdown.Replace("\\n", "\n");
        using var reader = new StringReader(normalizedMarkdown);

        while (reader.ReadLine() is { } line)
        {
            if (IsHeading(line, out var level, out var title))
            {
                if (HasOnlyWhitespace(buffer))
                {
                    buffer.Clear();
                    chunkHeadings = null;
                    chunkDocumentTitle = null;
                }

                while (headings.Count >= level)
                    headings.RemoveAt(headings.Count - 1);

                headings.Add(title);

                if (level == 1)
                    documentTitle = title;

                continue;
            }

            if (chunkHeadings is null)
            {
                chunkHeadings = headings.ToList();
                chunkDocumentTitle = documentTitle;
            }

            AppendLineAndFlushChunks(
                line,
                chunks,
                buffer,
                ref chunkHeadings,
                ref chunkDocumentTitle,
                documentTitle,
                documentId,
                sourceFile,
                language,
                ref chunkIndex,
                headings);
        }

        TryAddChunk(
            chunks,
            buffer,
            chunkHeadings,
            chunkDocumentTitle ?? documentTitle,
            documentId,
            sourceFile,
            language,
            ref chunkIndex);

        return chunks;
    }

    private static void TryAddChunk(
        ICollection<RegulationChunk> chunks,
        List<string> buffer,
        List<string>? chunkHeadings,
        string documentTitle,
        string documentId,
        string sourceFile,
        string language,
        ref int chunkIndex)
    {
        var chunkText = BuildChunkText(buffer);

        if (string.IsNullOrWhiteSpace(chunkText))
            return;

        var effectiveHeadings = chunkHeadings ?? [];
        var headingPath = string.Join(" > ", effectiveHeadings);
        var embeddingText =
$"""
Document: {documentTitle}
Path: {headingPath}

Text:
{chunkText}
""";

        var hash = ChunkHashService.CreateHash(documentId + headingPath + chunkText);

        chunks.Add(new RegulationChunk
        {
            Id = $"{documentId}_chunk_{chunkIndex:D5}",
            DocumentId = documentId,
            DocumentTitle = documentTitle,
            ChunkIndex = chunkIndex,
            Headings = effectiveHeadings.ToList(),
            HeadingPath = headingPath,
            HeadingLevel = effectiveHeadings.Count,
            ChunkText = chunkText,
            EmbeddingText = embeddingText,
            Language = language,
            SourceFile = sourceFile,
            Hash = hash
        });

        chunkIndex++;
    }

    private static void AppendLineAndFlushChunks(
        string line,
        ICollection<RegulationChunk> chunks,
        List<string> buffer,
        ref List<string>? chunkHeadings,
        ref string? chunkDocumentTitle,
        string documentTitle,
        string documentId,
        string sourceFile,
        string language,
        ref int chunkIndex,
        List<string> headings)
    {
        var remaining = line;

        while (true)
        {
            var markerIndex = remaining.IndexOf(ChunkEndMarker, StringComparison.Ordinal);

            if (markerIndex < 0)
            {
                buffer.Add(remaining);
                return;
            }

            buffer.Add(remaining[..markerIndex]);

            TryAddChunk(
                chunks,
                buffer,
                chunkHeadings,
                chunkDocumentTitle ?? documentTitle,
                documentId,
                sourceFile,
                language,
                ref chunkIndex);

            buffer.Clear();
            chunkHeadings = null;
            chunkDocumentTitle = null;

            remaining = remaining[(markerIndex + ChunkEndMarker.Length)..];

            if (remaining.Length == 0)
                return;

            chunkHeadings = headings.ToList();
            chunkDocumentTitle = documentTitle;
        }
    }

    private static string BuildChunkText(List<string> buffer)
    {
        var start = 0;
        var end = buffer.Count - 1;

        while (start <= end && string.IsNullOrWhiteSpace(buffer[start]))
            start++;

        while (end >= start && string.IsNullOrWhiteSpace(buffer[end]))
            end--;

        if (start > end)
            return string.Empty;

        return string.Join("\n", buffer.GetRange(start, end - start + 1));
    }

    private static bool HasOnlyWhitespace(List<string> buffer)
    {
        foreach (var line in buffer)
        {
            if (!string.IsNullOrWhiteSpace(line))
                return false;
        }

        return true;
    }

    private static bool IsHeading(string line, out int level, out string title)
    {
        level = 0;
        title = string.Empty;

        var trimmed = line.Trim();

        if (!trimmed.StartsWith("#", StringComparison.Ordinal))
            return false;

        var hashCount = 0;

        while (hashCount < trimmed.Length && trimmed[hashCount] == '#')
            hashCount++;

        if (hashCount < 1 || hashCount > 6)
            return false;

        if (trimmed.Length <= hashCount || trimmed[hashCount] != ' ')
            return false;

        level = hashCount;
        title = trimmed[(hashCount + 1)..].Trim();

        return !string.IsNullOrWhiteSpace(title);
    }
}
