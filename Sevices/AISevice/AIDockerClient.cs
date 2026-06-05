//using System.Net.Http.Json;
//using System.Text.Json;

//namespace LlamaParserV2;

//public sealed class AIDockerClient
//{
//    private readonly string _model = "ai/qwen3.5:4B-UD-Q4_K_XL";
//    private readonly string _url = "http://localhost:12434/engines/llama.cpp/v1/chat/completions";
//    private readonly double _temperature = 0.1;
//    private readonly int _contextSize = 8000;

//    public async Task<string> DocumentNormalization(
//        string document,
//        CancellationToken ct = default)
//    {
//        using var http = new HttpClient
//        {
//            Timeout = TimeSpan.FromMinutes(20)
//        };

//        var documentSystemProcessor = new SystemDocumentProcessor();
//        /*
//        var request = new
//        {
//            model = _model,
//            stream = false,
//            messages = new[]
//            {
//                new
//                {
//                    role = "system",
//                    content = documentSystemProcessor.Prompt
//                },
//                new
//                {
//                    role = "user",
//                    content = document + "\n\n/no_think"
//                }
//            },

//            //num_ctx = _contextSize,
//            //num_predict = 2048,
//            temperature = _temperature,
//            context = _contextSize
//        };*/

//        var request = new
//        {
//            model = _model,
//            stream = true,
//            messages = new[]
//        {
//        new { role = "system", content = documentSystemProcessor.Prompt },
//        new { role = "user", content = document + "\n\n/no_think" }
//        },
//            temperature = _temperature,
//            max_tokens = 12000,
//            context = _contextSize
//        };

//        using var response = await http.PostAsJsonAsync(_url, request, ct);
//        var json = await response.Content.ReadAsStringAsync(ct);

//        if (!response.IsSuccessStatusCode)
//            throw new HttpRequestException(json);

//        using var doc = JsonDocument.Parse(json);

//        return doc.RootElement.GetProperty("choices")[0]
//    .GetProperty("message")
//    .GetProperty("content")
//    .GetString() ?? string.Empty;
//    }
//}
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json;



namespace LlamaParserV2.Sevices.AISevice;


class AIDockerClient
{   //ai/qwen3.5:4B-UD-Q4_K_XL
    //qwen2.5:3B-Q4_K_M
    //ai/llama3.2:latest
    readonly string _model = "ai/llama3.2:latest";
    readonly double _temperature = 0.1;
    readonly string _localhostAddress = "http://localhost:12434/engines/llama.cpp/v1/chat/completions";
    readonly bool _stream = false;


    public AIDockerClient() { }
    public AIDockerClient(string model, string localHost, double temperature, bool stream)
    {
        _model = model;
        _localhostAddress = localHost;
        _temperature = temperature;
        _stream = stream;
    }

    public async Task<string> DocumentNormalization(string document, CancellationToken ct = default)
    {
        var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10)};
        var documentSystemProcessor = new SystemDocumentProcessor();
        var request = new
        {
            model = _model,
            messages = new[]
            {
                new
                {
                    role = documentSystemProcessor.Role,
                    content = documentSystemProcessor.Prompt
                },
                new
                {
                    role = "user",
                    content = "Normolize this document:\n"+"<document>"+document + "</document>"
                }
            },
            temperature = _temperature,
            stream = _stream,
            //max_tokens = 500,
            top_p = 0.9,
        };

        //var response = await http.PostAsJsonAsync(_localhostAddress, request);

        //string result = await response.Content.ReadAsStringAsync();

        var response = await http.PostAsJsonAsync(_localhostAddress, request);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response);
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        string json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);

        Console.WriteLine(json); 

        string? content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content ?? string.Empty;


        //return result;
    }
}

