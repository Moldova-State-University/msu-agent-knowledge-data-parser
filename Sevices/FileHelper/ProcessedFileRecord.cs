// NEW FILE
namespace LlamaParserV2.Sevices.FileHelper;

// Stores data about a PDF file that has already been processed.
public sealed class ProcessedFileRecord
{
    // Original PDF file name.
    public string FileName { get; set; } = string.Empty;
    // Relative path to the file inside the Storage/Raw folder.
    public string RelativePath { get; set; } = string.Empty;
    // Unique SHA-256 hash of the file contents.
    public string Sha256 { get; set; } = string.Empty;
    // File size in bytes.
    public long Size { get; set; }
    // Last write time of the file in UTC.
    public DateTime LastWriteTimeUtc { get; set; }
    // Time when the file was successfully processed.
    public DateTime ProcessedAtUtc { get; set; }
    // Name of the markdown file created from the PDF.
    public string MarkdownFileName { get; set; } = string.Empty;
}
