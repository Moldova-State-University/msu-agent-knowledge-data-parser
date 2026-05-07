// NEW FILE
using System.Security.Cryptography;

namespace LlamaParserV2;

// Contains helper method to compute a file hash.
public static class FileHashHelper
{
    // Computes the SHA-256 hash of a file and returns it as a string.
    public static async Task<string> ComputeSha256Async(string filePath)
    {
        // Opens the file for reading.
        await using var stream = File.OpenRead(filePath);
        // Computes the hash from the file contents.
        var hash = await SHA256.HashDataAsync(stream);
        // Converts the byte array to a hex string like A1B2C3...
        return Convert.ToHexString(hash);
    }
}
