using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LlamaParserV2.Sevices.LlamaService;

// Client for sending files and requests to the LlamaParse API.
public sealed class LlamaParseClient
{
    // Base URL for the LlamaParse API.
    private const string BaseUrl = "https://api.cloud.llamaindex.ai/api/v2/parse";

    // API key for accessing the service.
    private const string ApiKey = "llx-fLPsHvVzzKeKIgo0E5cQ5hMxFGpog9ARExQIelUAs5unCKyD";

    // HTTP client used to make requests.
    private readonly HttpClient _httpClient;

    // JSON settings for serialization and deserialization.
    private readonly JsonSerializerOptions _jsonOptions;

    // Configures the client and adds headers for API usage.
    public LlamaParseClient(HttpClient httpClient, string apiKey = ApiKey)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("LlamaParse API key is required.", nameof(apiKey));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    // Starts parsing an already uploaded file by its file_id.
    public async Task<LlamaParseStartResponse> ParseByFileIdAsync(
        string fileId,
        LlamaParseConfiguration configuration,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileId))
            throw new ArgumentException("fileId is required.", nameof(fileId));

        ArgumentNullException.ThrowIfNull(configuration);

        var request = new LlamaParseJsonRequest
        {
            FileId = fileId,
            Tier = configuration.Tier,
            Version = configuration.Version,
            ProcessingOptions = configuration.ProcessingOptions,
            AgenticOptions = configuration.AgenticOptions,
            WebhookConfigurations = configuration.WebhookConfigurations,
            InputOptions = configuration.InputOptions,
            CropBox = configuration.CropBox,
            PageRanges = configuration.PageRanges,
            DisableCache = configuration.DisableCache,
            OutputOptions = configuration.OutputOptions,
            ProcessingControl = configuration.ProcessingControl
        };

        return await PostJsonAsync<LlamaParseStartResponse>(BaseUrl, request, ct);
    }

    // Starts parsing a file by its source URL.
    public async Task<LlamaParseStartResponse> ParseByUrlAsync(
        string sourceUrl,
        LlamaParseConfiguration configuration,
        string? httpProxy = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sourceUrl))
            throw new ArgumentException("sourceUrl is required.", nameof(sourceUrl));

        ArgumentNullException.ThrowIfNull(configuration);

        var request = new LlamaParseJsonRequest
        {
            SourceUrl = sourceUrl,
            HttpProxy = httpProxy,
            Tier = configuration.Tier,
            Version = configuration.Version,
            ProcessingOptions = configuration.ProcessingOptions,
            AgenticOptions = configuration.AgenticOptions,
            WebhookConfigurations = configuration.WebhookConfigurations,
            InputOptions = configuration.InputOptions,
            CropBox = configuration.CropBox,
            PageRanges = configuration.PageRanges,
            DisableCache = configuration.DisableCache,
            OutputOptions = configuration.OutputOptions,
            ProcessingControl = configuration.ProcessingControl
        };

        return await PostJsonAsync<LlamaParseStartResponse>(BaseUrl, request, ct);
    }

    // Uploads a local file to LlamaParse and starts processing it immediately.
    public async Task<LlamaParseStartResponse> UploadAndParseAsync(
        string filePath,
        LlamaParseConfiguration configuration,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("filePath is required.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File for LlamaParse upload was not found.", filePath);

        ArgumentNullException.ThrowIfNull(configuration);

        // Opens the file for upload.
        await using var stream = File.OpenRead(filePath);

        // Builds a multipart request with the file and configuration.
        using var form = new MultipartFormDataContent();

        // Adds the file content to the form.
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "file", Path.GetFileName(filePath));

        // Adds JSON configuration to the form.
        var configJson = JsonSerializer.Serialize(configuration, _jsonOptions);
        form.Add(new StringContent(configJson, Encoding.UTF8), "configuration");

        // Sends the upload request.
        using var response = await _httpClient.PostAsync($"{BaseUrl}/upload", form, ct);
        await EnsureSuccessAsync(response, ct);

        // Reads and deserializes the API response.
        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<LlamaParseStartResponse>(json, _jsonOptions);

        if (result is null)
            throw new InvalidOperationException("LlamaParse returned an empty upload response.");

        return result;
    }

    // Gets the current result of a job by its jobId.
    public async Task<LlamaParseJobResultResponse> GetJobResultAsync(
        string jobId,
        IEnumerable<string>? expand = null,
        IEnumerable<string>? imageFileNames = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            throw new ArgumentException("jobId is required.", nameof(jobId));

        // Builds the list of query parameters.
        var query = new List<string>();

        // Adds the list of expandable fields if provided.
        if (expand is not null)
        {
            var expandValue = string.Join(",", expand.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (!string.IsNullOrWhiteSpace(expandValue))
                query.Add($"expand={Uri.EscapeDataString(expandValue)}");
        }

        // Adds the list of image file names if provided.
        if (imageFileNames is not null)
        {
            var fileNames = string.Join(",", imageFileNames.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (!string.IsNullOrWhiteSpace(fileNames))
                query.Add($"image_filenames={Uri.EscapeDataString(fileNames)}");
        }

        // Builds the final URL.
        var url = $"{BaseUrl}/{jobId}";
        if (query.Count > 0)
            url += "?" + string.Join("&", query);

        // Sends the request to get the result.
        using var response = await _httpClient.GetAsync(url, ct);
        await EnsureSuccessAsync(response, ct);

        // Reads and deserializes the API response.
        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<LlamaParseJobResultResponse>(json, _jsonOptions);

        if (result is null)
            throw new InvalidOperationException("LlamaParse returned an empty job result response.");

        return result;
    }

    // Returns combined markdown for all pages of the job.
    public async Task<string?> GetMarkdownAsync(string jobId, CancellationToken ct = default)
    {
        var result = await GetJobResultAsync(jobId, ["markdown"], ct: ct);

        // If the job is not yet completed, markdown is not available.
        if (!string.Equals(result.Job?.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
            return null;

        // If there are no markdown pages, return null.
        if (result.Markdown?.Pages is null || result.Markdown.Pages.Count == 0)
            return null;

        // Concatenates markdown from all pages in the correct order.
        return string.Join(
            Environment.NewLine + Environment.NewLine,
            result.Markdown.Pages
                .OrderBy(p => p.PageNumber)
                .Select(p => p.Markdown)
                .Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    // Returns combined text for all pages of the job.
    public async Task<string?> GetTextAsync(string jobId, CancellationToken ct = default)
    {
        var result = await GetJobResultAsync(jobId, ["text"], ct: ct);

        // If the job is not yet completed, text is not available.
        if (!string.Equals(result.Job?.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
            return null;

        // If there are no text pages, return null.
        if (result.Text?.Pages is null || result.Text.Pages.Count == 0)
            return null;

        // Concatenates text from all pages in the correct order.
        return string.Join(
            Environment.NewLine + Environment.NewLine,
            result.Text.Pages
                .OrderBy(p => p.PageNumber)
                .Select(p => p.Text)
                .Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    // Returns a list of LlamaParse jobs.
    public async Task<LlamaParseJobsListResponse> ListJobsAsync(
        int? pageSize = null,
        string? pageToken = null,
        string? status = null,
        CancellationToken ct = default)
    {
        // Builds parameters for filtering and pagination.
        var query = new List<string>();

        if (pageSize.HasValue)
            query.Add($"page_size={pageSize.Value}");

        if (!string.IsNullOrWhiteSpace(pageToken))
            query.Add($"page_token={Uri.EscapeDataString(pageToken)}");

        if (!string.IsNullOrWhiteSpace(status))
            query.Add($"status={Uri.EscapeDataString(status)}");

        // Builds the final URL.
        var url = BaseUrl;
        if (query.Count > 0)
            url += "?" + string.Join("&", query);

        // Requests the jobs list.
        using var response = await _httpClient.GetAsync(url, ct);
        await EnsureSuccessAsync(response, ct);

        // Reads and deserializes the API response.
        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<LlamaParseJobsListResponse>(json, _jsonOptions);

        if (result is null)
            throw new InvalidOperationException("LlamaParse returned an empty jobs list response.");

        return result;
    }

    // Polls the API until the job completes.
    public async Task<LlamaParseJobResultResponse> WaitForCompletionAsync(
        string jobId,
        IEnumerable<string>? expand = null,
        TimeSpan? pollInterval = null,
        int maxAttempts = 120,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            throw new ArgumentException("jobId is required.", nameof(jobId));

        if (maxAttempts <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxAttempts));

        // Uses the provided polling interval or the default.
        var delay = pollInterval ?? TimeSpan.FromSeconds(2);

        // Repeats fetching the result until the job completes or attempts run out.
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            var result = await GetJobResultAsync(jobId, expand, ct: ct);

            var status = result.Job?.Status;

            // Returns the result if the job reached any final status.
            if (status is "COMPLETED" or "FAILED" or "CANCELLED")
                return result;

            await Task.Delay(delay, ct);
        }

        throw new TimeoutException($"LlamaParse job '{jobId}' did not complete in time.");
    }

    // Sends a POST request with a JSON body and deserializes the response to the specified type.
    private async Task<T> PostJsonAsync<T>(string url, object body, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(body, _jsonOptions);

        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(url, content, ct);

        await EnsureSuccessAsync(response, ct);

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);

        if (result is null)
            throw new InvalidOperationException("LlamaParse returned an empty response.");

        return result;
    }

    // Ensures that the HTTP request completed successfully.
    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        // Reads error text from the response and throws a descriptive exception.
        var content = await response.Content.ReadAsStringAsync(ct);

        throw new HttpRequestException(
            $"LlamaParse request failed. StatusCode={(int)response.StatusCode}, Body={content}");
    }
}
