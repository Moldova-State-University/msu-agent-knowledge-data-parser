// MODIFIED
using LlamaParserV2;

// Creates an HTTP client for API requests.
var httpClient = new HttpClient();

// Creates a LlamaParse client.
var client = new LlamaParseClient(httpClient);

// Finds the project root folder.
var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

// Folder to read PDF files from.
var rawDirectory = Path.Combine(projectRoot, "Storage", "Raw");

// Folder to save markdown files to.
var markdownDirectory = Path.Combine(projectRoot, "Storage", "Markdown");

// Service for working with the JSON cache.
var processedFilesStore = new ProcessedFilesStore(Path.Combine(projectRoot, "Storage", "state", "processed-files.json"));

// Loads already processed files from the cache.
var processedFiles = await processedFilesStore.LoadAsync();

// Parsing configuration for LlamaParse.
var config = new LlamaParseConfiguration
{
    Tier = "cost_effective",
    Version = "latest",
    DisableCache = false,



    AgenticOptions = new AgenticOptions
    {
        CustomPrompt = "Convert the document into clean, strictly structured Markdown optimized for semantic chunking.\r\n\r\nSTRUCTURE REQUIREMENTS:\r\n- The document title at the beginning must appear exactly once as a level 1 heading: \"# \".\r\n- Chapter titles must be level 2 headings: \"## \".\r\n- Section or subsection titles must be level 3 headings: \"### \".\r\n- Preserve the semantic hierarchy of the document.\r\n- Do not skip heading levels.\r\n\r\nCONTENT CLEANING:\r\nCompletely remove and never include:\r\n- page numbers\r\n- standalone numeric lines\r\n- headers and footers\r\n- repeated page elements\r\n- line numbers\r\n- scanning/OCR artifacts\r\n- decorative separators\r\n- marginal notes\r\n\r\nIMPORTANT CLEANING RULES:\r\n- If a line contains only a number, remove it.\r\n- If short numeric artifacts repeat, remove them.\r\n- Keep valid numbered legal/article references such as \"Art. 127.\".\r\n\r\nPARAGRAPH RULES (CRITICAL):\r\n- Preserve logical paragraphs, not visual line breaks.\r\n- Insert the literal marker \"\\n\" ONLY at the end of a complete paragraph.\r\n- Do NOT insert \"\\n\" after every line.\r\n- Do NOT insert \"\\n\" after headings.\r\n- Do NOT insert \"\\n\" after each list item unless the list item itself is a full independent paragraph in the original document.\r\n- Lines that belong to the same paragraph must be merged into one continuous paragraph.\r\n- Wrapped lines caused by page layout or PDF formatting must be reconstructed into a single paragraph.\r\n\r\nLIST RULES:\r\n- Preserve ordered and unordered lists as Markdown lists.\r\n- Treat a list as a single structural block unless list items contain multiple independent paragraphs.\r\n- Do NOT append \"\\n\" after every bullet or lettered item.\r\n- Only append \"\\n\" after the full list block ends, if the list is followed by a new paragraph.\r\n\r\nSTRUCTURE INTEGRITY:\r\n- Preserve reading order across page breaks.\r\n- Do not lose or skip content.\r\n- Keep article numbering in correct sequence.\r\n\r\nOUTPUT EXAMPLE:\r\n# Document Title\r\n\r\n## Chapter Title\r\n\r\n### Section Title\r\n\r\nParagraph text continues as one logical paragraph and ends here.\\n\r\n\r\n**Art. 127.** Sunt pasibili de exmatriculare studenţii care:\r\na) au acumulat un deficit de credite de studii mai mare de 20;\r\nb) au absenţe nemotivate la 1/3 din numărul de ore contact direct prevăzute în planul de învăţământ;\r\nc) pentru încălcări grave a Cartei universitare şi Codului de etică și Integritate Academică al USM;\r\nd) din motive de sănătate;\r\ne) din cauza fraudării probelor de examinare;\r\nf) pentru neachitarea taxei de studii;\r\ng) din proprie iniţiativă.\\n\r\n\r\nFINAL RULE:\r\nReturn only the final Markdown content."
    },


    OutputOptions = new OutputOptions
    {
        Markdown = new MarkdownOutputOptions
        {

            AnnotateLinks = false,
            Tables = new MarkdownTablesOptions
            {
                OutputTablesAsMarkdown = true,
                MergeContinuedTables = false
            }
        }
    },
    ProcessingOptions = new ProcessingOptions
    {

        Ignore = new IgnoreOptions
        {

            IgnoreTextInImage = true,
            IgnoreDiagonalText = true,
        },
        OcrParameters = new OcrParameters
        {
            Languages = ["ro"],
        }
    }
};

// Creates the PDF folder if it does not exist.
Directory.CreateDirectory(rawDirectory);

// Creates the folder for markdown files if it does not exist.
Directory.CreateDirectory(markdownDirectory);

// Finds all PDF files in all subdirectories.
var pdfFiles = Directory.EnumerateFiles(rawDirectory, "*.pdf", SearchOption.AllDirectories);

// Counts how many new files were processed in this run.
var processedFilesCount = 0;

// Processes each found PDF file.
foreach (var pdfFilePath in pdfFiles)
{
    // Computes the file hash to check the cache.
    var sha256 = await FileHashHelper.ComputeSha256Async(pdfFilePath);

    // If the file is already in the cache, skip processing.
    if (processedFiles.Any(x => string.Equals(x.Sha256, sha256, StringComparison.OrdinalIgnoreCase)))
    {
        Console.WriteLine($"Skipping already processed file: {pdfFilePath}");
        continue;
    }

    // Sends the PDF to LlamaParse.
    var start = await client.UploadAndParseAsync(pdfFilePath, config);

    // Gets the job identifier.
    var jobId = start.JobId ?? start.Id;

    // If no job id was returned, stop execution.
    if (string.IsNullOrWhiteSpace(jobId))
        throw new Exception("Job id was not returned.");

    // Waits for the file processing to complete.
    var result = await client.WaitForCompletionAsync(jobId, ["text", "markdown"]);

    // Shows job status and document name.
    Console.WriteLine($"Status: {result.Job?.Status}");
    Console.WriteLine($"Document: {result.Job?.Name}");

    // You can enable printing the first page text here if needed.
    //var firstTextPage = result.Text?.Pages.FirstOrDefault();
    //Console.WriteLine(firstTextPage?.Text);

    // Retrieves the final markdown from the result.
    var markdown = await client.GetMarkdownAsync(jobId);

    // Forms the markdown file name from the PDF name.
    var markdownFileName = $"{Path.GetFileNameWithoutExtension(pdfFilePath)}.md";

    // Builds the full path to the markdown file.
    var markdownFilePath = Path.Combine(markdownDirectory, markdownFileName);

    // Saves the markdown to disk.
    await File.WriteAllTextAsync(markdownFilePath, markdown ?? string.Empty);

    // You can enable printing the whole markdown to the console here if needed.
    //Console.WriteLine(markdown);

    // Additionally protects against adding the same hash twice.
    if (processedFiles.Any(x => string.Equals(x.Sha256, sha256, StringComparison.OrdinalIgnoreCase)))
    {
        continue;
    }

    // Reads file size and last modified date.
    var fileInfo = new FileInfo(pdfFilePath);

    // Adds a new record for the file that was just processed.
    processedFiles.Add(new ProcessedFileRecord
    {
        FileName = Path.GetFileName(pdfFilePath),
        RelativePath = Path.GetRelativePath(rawDirectory, pdfFilePath),
        Sha256 = sha256,
        Size = fileInfo.Length,
        LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,
        ProcessedAtUtc = DateTime.UtcNow,
        MarkdownFileName = markdownFileName
    });

    // Saves the updated cache to JSON.
    await processedFilesStore.SaveAsync(processedFiles);

    // Increments the count of newly processed files.
    processedFilesCount++;

    // Shows that a new cache record was added.
    Console.WriteLine($"Added new cache record: {pdfFilePath}");
}

// If there were no new files, print a clear message.
if (processedFilesCount == 0)
{
    Console.WriteLine("No new PDF files were parsed in this run.");
}
