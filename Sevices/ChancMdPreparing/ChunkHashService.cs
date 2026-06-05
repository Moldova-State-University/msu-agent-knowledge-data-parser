using System;
using System.Collections.Generic;
using System.Text;

namespace LlamaParserV2.Sevices.ChancMdPreparing;

using System.Security.Cryptography;
using System.Text;

public static class ChunkHashService
{
    public static string CreateHash(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}