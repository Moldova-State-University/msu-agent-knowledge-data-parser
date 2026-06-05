// NEW FILE
using System.Text.Json;

namespace LlamaParserV2.Sevices.FileHelper;

// Reads and saves the processed files cache to a JSON file.
public sealed class ProcessedFilesStore
{
    // JSON settings to keep the file human-readable.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    // Path to the JSON cache file.
    private readonly string _filePath;

    // Stores the path to the cache file.
    public ProcessedFilesStore(string filePath)
    {
        _filePath = filePath;
    }

    // Loads the list of already processed files.
    public async Task<List<ProcessedFileRecord>> LoadAsync()
    {
        // Create the directory for the cache file if it does not exist.
        EnsureDirectory();

        // If the cache does not exist yet, create an empty file and return an empty list.
        if (!File.Exists(_filePath))
        {
            await SaveAsync([]);
            return [];
        }

        try
        {
            // Read JSON from the file.
            var json = await File.ReadAllTextAsync(_filePath);

            // If the file is empty, there are no records yet.
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            // Deserialize JSON into a list of records.
            return JsonSerializer.Deserialize<List<ProcessedFileRecord>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            // If JSON is corrupted, start with an empty cache.
            return [];
        }
        catch (IOException)
        {
            // If the file cannot be read right now, continue with an empty cache.
            return [];
        }
    }

    // Saves the list of processed files to JSON.
    public async Task SaveAsync(List<ProcessedFileRecord> records)
    {
        // Create the directory for the cache file if it does not exist.
        EnsureDirectory();
        // Serialize the list of records to a JSON string.
        var json = JsonSerializer.Serialize(records, JsonOptions);
        // Write the JSON to the file.
        await File.WriteAllTextAsync(_filePath, json);
    }

    // Ensures that the directory for the cache file exists.
    private void EnsureDirectory()
    {
        // Get the directory path where the JSON file should reside.
        var directoryPath = Path.GetDirectoryName(_filePath);

        // Create the directory if the path is valid.
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
}
