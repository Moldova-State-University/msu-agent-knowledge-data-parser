using System.Text.Json.Serialization;

namespace LlamaParserV2;

// The request body sent to the LlamaParse API.
public sealed class LlamaParseJsonRequest
{
    // Identifier of an already uploaded file.
    [JsonPropertyName("file_id")]
    public string? FileId { get; set; }

    // URL of the file to process.
    [JsonPropertyName("source_url")]
    public string? SourceUrl { get; set; }

    // Proxy to use when downloading the file by URL, if needed.
    [JsonPropertyName("http_proxy")]
    public string? HttpProxy { get; set; }

    // Processing tier.
    [JsonPropertyName("tier")]
    public string Tier { get; set; } = "cost_effective";

    // Parser version.
    [JsonPropertyName("version")]
    public string Version { get; set; } = "latest";

    // Document processing options.
    [JsonPropertyName("processing_options")]
    public ProcessingOptions? ProcessingOptions { get; set; }

    // Additional agentic/parsing options.
    [JsonPropertyName("agentic_options")]
    public AgenticOptions? AgenticOptions { get; set; }

    // Webhook notification settings.
    [JsonPropertyName("webhook_configurations")]
    public List<WebhookConfiguration>? WebhookConfigurations { get; set; }

    // Input document type settings.
    [JsonPropertyName("input_options")]
    public InputOptions? InputOptions { get; set; }

    // Crop box area of the page to process.
    [JsonPropertyName("crop_box")]
    public CropBox? CropBox { get; set; }

    // Page range limits.
    [JsonPropertyName("page_ranges")]
    public PageRanges? PageRanges { get; set; }

    // Flag to disable server-side LlamaParse cache.
    [JsonPropertyName("disable_cache")]
    public bool? DisableCache { get; set; }

    // Output format settings.
    [JsonPropertyName("output_options")]
    public OutputOptions? OutputOptions { get; set; }

    // Additional processing control options.
    [JsonPropertyName("processing_control")]
    public ProcessingControl? ProcessingControl { get; set; }
}

// Main configuration used to start parsing.
public sealed class LlamaParseConfiguration
{
    // Processing tier.
    [JsonPropertyName("tier")]
    public string Tier { get; set; } = "cost_effective";

    // Parser version.
    [JsonPropertyName("version")]
    public string Version { get; set; } = "latest";

    // Document processing options.
    [JsonPropertyName("processing_options")]
    public ProcessingOptions? ProcessingOptions { get; set; }

    // Additional agentic/parsing options.
    [JsonPropertyName("agentic_options")]
    public AgenticOptions? AgenticOptions { get; set; }

    // Webhook notification settings.
    [JsonPropertyName("webhook_configurations")]
    public List<WebhookConfiguration>? WebhookConfigurations { get; set; }

    // Input document type settings.
    [JsonPropertyName("input_options")]
    public InputOptions? InputOptions { get; set; }

    // Crop box area of the page to process.
    [JsonPropertyName("crop_box")]
    public CropBox? CropBox { get; set; }

    // Page range limits.
    [JsonPropertyName("page_ranges")]
    public PageRanges? PageRanges { get; set; }

    // Flag to disable server-side LlamaParse cache.
    [JsonPropertyName("disable_cache")]
    public bool? DisableCache { get; set; }

    // Output format settings.
    [JsonPropertyName("output_options")]
    public OutputOptions? OutputOptions { get; set; }

    // Additional processing control options.
    [JsonPropertyName("processing_control")]
    public ProcessingControl? ProcessingControl { get; set; }
}

// Common document processing settings.
public sealed class ProcessingOptions
{
    // Rules for ignoring unwanted text.
    [JsonPropertyName("ignore")]
    public IgnoreOptions? Ignore { get; set; }

    // OCR settings.
    [JsonPropertyName("ocr_parameters")]
    public OcrParameters? OcrParameters { get; set; }
}

// Options for what to ignore during processing.
public sealed class IgnoreOptions
{
    // Ignore diagonal text.
    [JsonPropertyName("ignore_diagonal_text")]
    public bool? IgnoreDiagonalText { get; set; }

    // Ignore text inside images.
    [JsonPropertyName("ignore_text_in_image")]
    public bool? IgnoreTextInImage { get; set; }
}

// OCR parameters for text recognition.
public sealed class OcrParameters
{
    // List of OCR languages.
    [JsonPropertyName("languages")]
    public List<string>? Languages { get; set; }
}

// Agentic options for custom model behavior during parsing.
public sealed class AgenticOptions
{
    // Custom prompt to improve output structure.
    [JsonPropertyName("custom_prompt")]
    public string? CustomPrompt { get; set; }
}

// Webhook settings for event notifications.
public sealed class WebhookConfiguration
{
    // Webhook URL where the service will send notifications.
    [JsonPropertyName("webhook_url")]
    public string WebhookUrl { get; set; } = default!;

    // Headers to be sent with the webhook.
    [JsonPropertyName("webhook_headers")]
    public Dictionary<string, string>? WebhookHeaders { get; set; }

    // List of events that trigger webhooks.
    [JsonPropertyName("webhook_events")]
    public List<string>? WebhookEvents { get; set; }
}

// Settings for different input document types.
public sealed class InputOptions
{
    // Settings for HTML documents.
    [JsonPropertyName("html")]
    public HtmlInputOptions? Html { get; set; }

    // Settings for spreadsheets and tables.
    [JsonPropertyName("spreadsheet")]
    public SpreadsheetInputOptions? Spreadsheet { get; set; }

    // Settings for presentations.
    [JsonPropertyName("presentation")]
    public PresentationInputOptions? Presentation { get; set; }
}

// Input options for HTML documents.
public sealed class HtmlInputOptions
{
    // Make all HTML elements visible before processing.
    [JsonPropertyName("make_all_elements_visible")]
    public bool? MakeAllElementsVisible { get; set; }

    // Remove fixed page elements.
    [JsonPropertyName("remove_fixed_elements")]
    public bool? RemoveFixedElements { get; set; }

    // Remove navigation elements.
    [JsonPropertyName("remove_navigation_elements")]
    public bool? RemoveNavigationElements { get; set; }
}

// Input options for spreadsheet documents.
public sealed class SpreadsheetInputOptions
{
    // Try to detect nested tables in sheets.
    [JsonPropertyName("detect_sub_tables_in_sheets")]
    public bool? DetectSubTablesInSheets { get; set; }

    // Force formula computation in sheets.
    [JsonPropertyName("force_formula_computation_in_sheets")]
    public bool? ForceFormulaComputationInSheets { get; set; }
}

// Input options for presentations.
public sealed class PresentationInputOptions
{
    // Process content outside the visible slide area.
    [JsonPropertyName("out_of_bounds_content")]
    public bool? OutOfBoundsContent { get; set; }
}

// Crop box area to keep for processing.
public sealed class CropBox
{
    // Top boundary of the crop box.
    [JsonPropertyName("top")]
    public double? Top { get; set; }

    // Right boundary of the crop box.
    [JsonPropertyName("right")]
    public double? Right { get; set; }

    // Bottom boundary of the crop box.
    [JsonPropertyName("bottom")]
    public double? Bottom { get; set; }

    // Left boundary of the crop box.
    [JsonPropertyName("left")]
    public double? Left { get; set; }
}

// Limits for page ranges to process.
public sealed class PageRanges
{
    // Maximum number of pages to process.
    [JsonPropertyName("max_pages")]
    public int? MaxPages { get; set; }

    // Specific pages or page ranges.
    [JsonPropertyName("target_pages")]
    public string? TargetPages { get; set; }
}

// Output format options.
public sealed class OutputOptions
{
    // Markdown output settings.
    [JsonPropertyName("markdown")]
    public MarkdownOutputOptions? Markdown { get; set; }

    // List of images to save separately.
    [JsonPropertyName("images_to_save")]
    public List<string>? ImagesToSave { get; set; }
}

// Markdown output options.
public sealed class MarkdownOutputOptions
{
    // Whether to annotate links.
    [JsonPropertyName("annotate_links")]
    public bool? AnnotateLinks { get; set; }

    // Table output settings.
    [JsonPropertyName("tables")]
    public MarkdownTablesOptions? Tables { get; set; }
}

// Options for converting tables to markdown.
public sealed class MarkdownTablesOptions
{
    // Make markdown tables compact.
    [JsonPropertyName("compact_markdown_tables")]
    public bool? CompactMarkdownTables { get; set; }

    // Convert tables to markdown format.
    [JsonPropertyName("output_tables_as_markdown")]
    public bool? OutputTablesAsMarkdown { get; set; }

    // Separator for multiline table cells.
    [JsonPropertyName("markdown_table_multiline_separator")]
    public string? MarkdownTableMultilineSeparator { get; set; }

    // Merge tables that continue across pages.
    [JsonPropertyName("merge_continued_tables")]
    public bool? MergeContinuedTables { get; set; }
}

// Additional processing control parameters.
public sealed class ProcessingControl
{
    // Timeout settings.
    [JsonPropertyName("timeouts")]
    public Timeouts? Timeouts { get; set; }

    // Conditions under which a job is considered failed.
    [JsonPropertyName("job_failure_conditions")]
    public JobFailureConditions? JobFailureConditions { get; set; }
}

// Processing timeout settings.
public sealed class Timeouts
{
    // Base timeout in seconds.
    [JsonPropertyName("base_in_seconds")]
    public int? BaseInSeconds { get; set; }

    // Extra time per page in seconds.
    [JsonPropertyName("extra_time_per_page_in_seconds")]
    public int? ExtraTimePerPageInSeconds { get; set; }
}

// Conditions that determine job failure.
public sealed class JobFailureConditions
{
    // Allowed fraction of pages with failures.
    [JsonPropertyName("allowed_page_failure_ratio")]
    public double? AllowedPageFailureRatio { get; set; }

    // Whether to fail on image extraction errors.
    [JsonPropertyName("fail_on_image_extraction_error")]
    public bool? FailOnImageExtractionError { get; set; }

    // Whether to fail on image OCR errors.
    [JsonPropertyName("fail_on_image_ocr_error")]
    public bool? FailOnImageOcrError { get; set; }

    // Whether to fail on markdown reconstruction errors.
    [JsonPropertyName("fail_on_markdown_reconstruction_error")]
    public bool? FailOnMarkdownReconstructionError { get; set; }

    // Whether to fail on buggy fonts.
    [JsonPropertyName("fail_on_buggy_font")]
    public bool? FailOnBuggyFont { get; set; }
}

// API response after starting a parse job.
public sealed class LlamaParseStartResponse
{
    // General response or job identifier.
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    // Parsing job identifier.
    [JsonPropertyName("job_id")]
    public string? JobId { get; set; }

    // Current job status.
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    // Project identifier in LlamaParse.
    [JsonPropertyName("project_id")]
    public string? ProjectId { get; set; }

    // Error message if present.
    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
}

// API response with a list of jobs.
public sealed class LlamaParseJobsListResponse
{
    // List of found jobs.
    [JsonPropertyName("items")]
    public List<LlamaParseJobInfo> Items { get; set; } = [];

    // Next page token for pagination.
    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }

    // Total number of jobs.
    [JsonPropertyName("total_size")]
    public int? TotalSize { get; set; }
}

// Brief information about a single job.
public sealed class LlamaParseJobInfo
{
    // Job identifier.
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    // Project identifier.
    [JsonPropertyName("project_id")]
    public string? ProjectId { get; set; }

    // Job status.
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    // Error message, if any.
    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
}

// Полный ответ API с результатом обработки задачи.
public sealed class LlamaParseJobResultResponse
{
    // Детали самой задачи.
    [JsonPropertyName("job")]
    public LlamaParseJobDetails? Job { get; set; }

    // Метаданные содержимого результата.
    [JsonPropertyName("result_content_metadata")]
    public object? ResultContentMetadata { get; set; }

    // Постраничный текстовый результат.
    [JsonPropertyName("text")]
    public LlamaParsePagedTextResult? Text { get; set; }

    // Постраничный markdown-результат.
    [JsonPropertyName("markdown")]
    public LlamaParsePagedMarkdownResult? Markdown { get; set; }

    // Дополнительные элементы результата.
    [JsonPropertyName("items")]
    public object? Items { get; set; }

    // Дополнительные метаданные.
    [JsonPropertyName("metadata")]
    public object? Metadata { get; set; }

    // Полный markdown одной строкой.
    [JsonPropertyName("markdown_full")]
    public string? MarkdownFull { get; set; }

    // Полный текст одной строкой.
    [JsonPropertyName("text_full")]
    public string? TextFull { get; set; }

    // Метаданные по изображениям.
    [JsonPropertyName("images_content_metadata")]
    public object? ImagesContentMetadata { get; set; }

    // Метаданные самой задачи.
    [JsonPropertyName("job_metadata")]
    public object? JobMetadata { get; set; }

    // Исходные параметры задачи.
    [JsonPropertyName("raw_parameters")]
    public object? RawParameters { get; set; }

    // Отладочная информация.
    [JsonPropertyName("debug")]
    public object? Debug { get; set; }
}

// Подробная информация о задаче парсинга.
public sealed class LlamaParseJobDetails
{
    // Идентификатор задачи.
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    // Время создания задачи.
    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    // Время последнего обновления задачи.
    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; set; }

    // Идентификатор проекта.
    [JsonPropertyName("project_id")]
    public string? ProjectId { get; set; }

    // Текущий статус задачи.
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    // Сообщение об ошибке, если оно есть.
    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    // Имя документа или задачи.
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    // Использованный тариф обработки.
    [JsonPropertyName("tier")]
    public string? Tier { get; set; }
}

// Постраничный текстовый результат.
public sealed class LlamaParsePagedTextResult
{
    // Список страниц с текстом.
    [JsonPropertyName("pages")]
    public List<LlamaParseTextPage> Pages { get; set; } = [];
}

// Текст одной страницы документа.
public sealed class LlamaParseTextPage
{
    // Номер страницы.
    [JsonPropertyName("page_number")]
    public int PageNumber { get; set; }

    // Текст страницы.
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

// Постраничный markdown-результат.
public sealed class LlamaParsePagedMarkdownResult
{
    // Список страниц с markdown.
    [JsonPropertyName("pages")]
    public List<LlamaParseMarkdownPage> Pages { get; set; } = [];
}

// Markdown одной страницы документа.
public sealed class LlamaParseMarkdownPage
{
    // Номер страницы.
    [JsonPropertyName("page_number")]
    public int PageNumber { get; set; }

    // Markdown-содержимое страницы.
    [JsonPropertyName("markdown")]
    public string? Markdown { get; set; }

    // Заголовок страницы, если он найден.
    [JsonPropertyName("header")]
    public string? Header { get; set; }

    // Нижний колонтитул страницы, если он найден.
    [JsonPropertyName("footer")]
    public string? Footer { get; set; }

    // Флаг успешной обработки страницы.
    [JsonPropertyName("success")]
    public bool? Success { get; set; }
}
