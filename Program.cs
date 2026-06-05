// MODIFIED
using LlamaParserV2;
using LlamaParserV2.Sevices.AISevice;
using LlamaParserV2.Sevices.ChancMdPreparing;
using LlamaParserV2.Sevices.FileHelper;
using LlamaParserV2.Sevices.LlamaService;
using System.Data.SqlTypes;


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

// Folder to save processed files by AI.
var processedDirectory = Path.Combine(projectRoot, "Storage", "Processed");

// Folder to save processed files by AI.
var chunkDirectory = Path.Combine(projectRoot, "Storage", "DocumentPreparedChunks");

// Service for working with the JSON cache.
var processedFilesStoreLlama = new ProcessedFilesStore(Path.Combine(projectRoot, "Storage", "state", "processed-files-Llama.json"));

var processedFilesStoreAi = new ProcessedFilesStore(Path.Combine(projectRoot, "Storage", "State", "processed-files-AI.json"));

// Loads already processed files from the cache.
var processedFilesLlama = await processedFilesStoreLlama.LoadAsync();

// Loads already processed files from the cache.
var processedFilesAi = await processedFilesStoreAi.LoadAsync();

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

// Counts how many new files were processed in markdown this run.
var processedToMdFilesCount = 0;

// Processes each found PDF file.
foreach (var pdfFilePath in pdfFiles)
{
    // Computes the file hash to check the cache.
    var sha256 = await FileHashHelper.ComputeSha256Async(pdfFilePath);

    // If the file is already in the cache, skip processing.
    if (processedFilesLlama.Any(x => string.Equals(x.Sha256, sha256, StringComparison.OrdinalIgnoreCase)))
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
    if (processedFilesLlama.Any(x => string.Equals(x.Sha256, sha256, StringComparison.OrdinalIgnoreCase)))
    {
        continue;
    }

    // Reads file size and last modified date.
    var fileInfo = new FileInfo(pdfFilePath);

    // Adds a new record for the file that was just processed.
    processedFilesLlama.Add(new ProcessedFileRecord
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
    await processedFilesStoreLlama.SaveAsync(processedFilesLlama);

    // Increments the count of newly processed files.
    processedToMdFilesCount++;

    // Shows that a new cache record was added.
    Console.WriteLine($"Added new cache record: {pdfFilePath}");
}

// If there were no new files, print a clear message.
if (processedToMdFilesCount == 0)
{
    Console.WriteLine("No new PDF files were parsed in this run.");
}


Console.WriteLine("\nAI TEXT PROCESSING STARTED\n");


// Counts how many new files were processed in markdown this run.
int processedByAiFilesCount = 0;

var mdFiles = Directory.EnumerateFiles(markdownDirectory, "*.md");


var aiDockerClient = new AIDockerClient();

//foreach (var mdFilePath in mdFiles)
//{

//    // Computes the file hash to check the cache.
//    var sha256 = await FileHashHelper.ComputeSha256Async(mdFilePath);

//    // If the file is already in the cache, skip processing.
//    if (processedFilesAi.Any(x => string.Equals(x.Sha256, sha256, StringComparison.OrdinalIgnoreCase)))
//    {
//        Console.WriteLine($"Skipping already processed file: {mdFilePath}");
//        continue;
//    }

//    var markdown = await aiDockerClient.DocumentNormalization(File.ReadAllText(mdFilePath));

//    if (markdown == string.Empty)
//    {
//        Console.WriteLine("Something went wrong. AI has returned empty content");
//        continue;
//    }

//    // Forms the markdown file name from the PDF name.
//    var processedFileName = $"{Path.GetFileNameWithoutExtension(mdFilePath)}.md";

//    // Builds the full path to the markdown file.
//    var processedFilePath = Path.Combine(processedDirectory, processedFileName);

//    // Saves the markdown to disk.
//    await File.WriteAllTextAsync(processedFilePath, markdown ?? string.Empty);

//    Console.WriteLine("New post processed file saved");


//    // Reads file size and last modified date.
//    var fileInfo = new FileInfo(mdFilePath);

//    // Adds a new record for the file that was just processed.
//    processedFilesAi.Add(new ProcessedFileRecord
//    {
//        FileName = Path.GetFileName(mdFilePath),
//        RelativePath = Path.GetRelativePath(markdownDirectory, mdFilePath),
//        Sha256 = sha256,
//        Size = fileInfo.Length,
//        LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,
//        ProcessedAtUtc = DateTime.UtcNow,
//        MarkdownFileName = processedFileName
//    });

//    // Saves the updated cache to JSON.
//    await processedFilesStoreAi.SaveAsync(processedFilesAi);

//    // Shows that a new cache record was added.
//    Console.WriteLine($"Added new cache record: {processedFilePath}");

//    // Increments the count of newly processed files.
//    processedByAiFilesCount++;
//}

//// If there were no new files, print a clear message.
//if (processedByAiFilesCount == 0)
//{
//    Console.WriteLine("No new MD files were reviewd by AI.");
//}

///////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////Testing LLm Postprocessing///////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////
//var testMarkdown = await aiDockerClient.DocumentNormalization("| ![Universitatea de Stat din Moldova Logo](page_1_image_1_v2.jpg) | **Universitatea de Stat din Moldova**                                                                                                      | **Organism emitent:**<br/>Departamentul Managementul Calității                                                                  |   |   |\r\n| ---------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------- | - | - |\r\n|                                                                  | **REGULAMENT**<br/>**PRIVIND FORMAREA PROFESIONALĂ LA CICLUL I, STUDII SUPERIOARE DE LICENȚĂ ÎN CADRUL UNIVERSITĂȚII DE STAT DIN MOLDOVA** | **APROBAT de Senatul USM**<br/>26 decembrie 2019,<br/>proces-verbal nr. 4<br/>Rector, prof. univ. dr. hab.<br/>Gheorghe Ciocanu |   |   |\r\n|                                                                  |                                                                                                                                            |                                                                                                                                 |   |   |\r\n\r\n<!-- layout: fidh, gaix, ooej -->\r\n![Official Seal of the Republic of Moldova and Ministry of Education and Research](page_1_seal_1_v2.jpg)\r\n\r\n## REGULAMENT PRIVIND FORMAREA PROFESIONALĂ LA CICLUL I, STUDII SUPERIOARE DE LICENȚĂ ÎN CADRUL UNIVERSITĂȚII DE STAT DIN MOLDOVA\r\n\r\n### CAPITOLUL I\r\n\r\n### DISPOZIȚII GENERALE\r\n\r\nArt. 1. Prezentul Regulament stabilește cadrul normativ ce reglementează procesul de formare profesională la ciclul I, studii superioare de licență, în cadrul Universității de Stat din Moldova.\\n\r\n\r\nArt. 2. Prezentul Regulament este elaborat în conformitate cu:\r\n\r\n* Codul Educației al Republicii Moldova, Legea nr. 152 din 17.07.2014;\r\n* Ghidul utilizatorului Sistemului European de Credite Trasferabile/ ECTS, 2015;\r\n* Nomenclatorul domeniilor de formare profesională şi al specialităților în învățământul superior, aprobat prin HG Nr. 482 din 28.06.2017;\r\n* Cadrul Național al Calificărilor din Republica Moldova, aprobat prin HG nr. 1016 din 23.11.2017;\r\n* Regulamentul cu privire la condiţiile de ocupare a locurilor cu finanţare bugetară în instituţiile de învăţământ superior de stat din Republica Moldova, Anexa la Ordinul ME nr. 748 din 12.07.2013;\r\n* Regulamentul cu privire la mobilitatea academică în învățământul superior, aprobat prin HG nr. 56 din 27.01.2014;\r\n* Regulamentul-cadru privind stagiile de practică în învățământul superior, Ordinul ME nr. 203 din 19.03.2014;\r\n* Regulamentul-cadru privind organizarea examenului de finalizare a studiilor superioare de licență, aprobat prin Ordinul ME nr. 1047 din 29.10.2015;\r\n* Regulamentul de organizare a studiilor superioare de licență (ciclul I) și intregate, Anexa la Ordinul MECC nr. 1625 din 12.12.2019;\r\n* Planul-cadru pentru studii superioare de licență (ciclul I), de master (ciclul II) și integrate, Anexa la Ordinul MECC nr. 120 din 10.02.2020;\r\n* Carta Universității de Stat din Moldova, aprobată prin decizia Senatului USM din 31.03.2015;\r\n* Regulament cu privire la condițiile de ocupare a locurilor cu finanțare bugetară la USM, aprobat prin decizia Senatului USM din 27.05.2014;\r\n* Regulamentul de aplicare a Sistemului Național de Credite de Studiu la USM, aprobat prin decizia Senatului USM din 25.02.2014;\r\n* Regulament instituțional privind evaluarea randamentului academic, aprobat prin decizia Senatului USM din 15.04.2014;\r\n* Regulament privind lichidarea restanțelor academice ale studenților Universității de Stat din Moldova, aporbat prin decizia Senatului USM din 25.11.2014;\r\n* Regulament cu privire la organizarea stagiilor de practică la ciclul I, studii superioare de licență, ciclul II, studii superioare de masterat, aprobat prin decizia Senatului USM din 26.01.2016.\r\n\r\n**Art. 3.** Procesul de formare profesională la ciclul I, studii superioare de licență reflectă viziunea și misiunea USM privind asigurarea realizării standardelor de calitate în domeniul serviciilor educaționale în formarea inițială a specialiştilor de înaltă calificare pentru economia națională și competitive pe piața muncii internațională.\\n\r\n\r\n**Art. 4.** Procesul de formare profesională la ciclul I, studii superioare de licență se realizează în cadrul Universităţii de Stat din Moldova (USM) în conformitate cu politica educaţională instituțională, cu accent pe orientarea finalităţilor de studii către cerinţele pieţei muncii şi formarea sistemului de competenţe necesar integrării socioprofesionale.\\n\r\n\r\n## CAPITOLUL II\r\n\r\n### Organizarea procesului de formare profesională\\n\r\n\r\n**Art. 5.** USM organizează procesul de formare profesională la ciclul I, studii superioare de licență la programele de studii acreditate sau autorizate provizoriu.\\n\r\n\r\n**Art. 6.** USM organizează procesul de formare profesională la ciclul I, studii superioare de licență prin următoarele forme de învăţământ:\\n\r\n\r\n* a) cu frecvenţă;\\n\r\n* b) cu frecvenţă redusă;\\n\r\n* c) la distanţă.\\n\r\n\r\n**Art. 7.** Formele de învățământ cu frecvenţă redusă şi la distanţă pot fi organizate doar pentru programele de formare profesională de la ciclul I, licenţă la care se realizează şi studii cu frecvenţă.\\n\r\n\r\n**Art. 8.** La programele de studii din domeniul de formare profesională 0313 Psihologie şi 0231 Studiul limbilor, studiile superioare de licenţă se organizează doar la forma de învăţământ cu frecvenţă.\\n\r\n\r\n**Art. 9.** USM organizează procesul de formare profesională, în cadrul studiilor superioare de licență, la specialități duble în domeniul general de studiu 011 Științe ale educației. Durata studiilor la aceste programe este mai mare cu un an.\\n\r\n\r\n### I. Programele de studii\\n\r\n\r\n**Art. 10.** Programele de studii reflectă concepţia şi conţinutul formării profesionale la ciclul I, studii superioare de licenţă și sunt actualizate periodic și incluse în oferta educațională a USM.\\n\r\n\r\n**Art. 11.** Programele de studii proiectează traseul de formare profesională al studentului, orientat spre realizarea finalităţilor de studii şi obţinerea calificării profesionale.\\n\r\n\r\n**Art. 12.** Programele de studii, realizate în cadrul USM, oferă procesului de formare profesională particularităţi distincte, determinate de tradiţiile ştiinţifico-didactice din instituţie şi experienţa şcolilor ştiinţifice din diverse domenii de cercetare.\\n\r\n\r\n**Art. 13.** Programele de formare profesională la ciclul I, studii superioare de licență, sunt conceptualizate în baza unei abordări curriculare, care reflectă tendinţele moderne ale procesului educaţional şi sunt orientate spre formarea competenţelor profesionale solicitate de piața muncii.\\n\r\n\r\n## II. Programele comune de studii\r\n\r\n**Art. 14.** USM organizează programe comune de formare profesională la ciclul I, licență în colaborare/ parteneriat cu alte instituții de învățământ superior naționale sau internaționale. Programele comune de studii superioare reprezintă o formă de colaborare dintre USM şi alte instituţii de învăţământ superior, realizate, de regulă, în cadrul unor consorţii.\\n\r\n\r\n**Art. 15.** Un program comun de studii superioare, organizat la USM, presupune că:\r\na) instituţiile membre ale consorţiului sunt autorizate provizoriu sau acreditate în ţara de origine;\r\nb) fiecare membru al consorţiului dispune de permisiunea autorităţilor naţionale abilitate în acest scop pentru organizarea programului comun.\\n\r\n\r\n**Art. 16.** Colaborarea în cadrul programelor comune de studii prevede:\r\na) elaborarea şi aprobarea programului de studii superioare de licenţă;\r\nb) organizarea admiterii;\r\nc) supervizarea academică, conferirea calificării şi asigurarea calităţii.\\n\r\n\r\n**Art. 17.** Acordarea calificării comune şi eliberarea diplomei comune se realizează în una din următoarele formule:\r\na) o diplomă comună suplimentară la diploma naţională;\r\nb) o diplomă comună emisă de către USM şi instituţiile partenere;\r\nc) o diplomă naţională eliberată oficial şi un certificat pentru atestarea calificării acordate în comun.\r\n\r\nDiplomele şi certificatele comune se perfectează în limbile de comunicare stabilite în cadrul parteneriatului şi în limba engleză.\\n\r\n\r\n**Art. 18.** Studenţii din fiecare instituţie participantă la programul comun, realizează câte o perioadă de studii în diferite instituţii partenere, dar nu obligatoriu în toate instituţiile consorţiului. Perioada de studii a participanţilor la program în cadrul instituţiilor sau organizaţiilor partenere ale instituţiei de învățământ superior, constituie o parte substanțială a programului comun (nu mai puțin de 30 de credite). Perioadele de studii și examenele promovate în cadrul instituțiilor partenere sunt recunoscute pe deplin și în mod expres, conform legislației în vigoare.\\n\r\n");
//Console.WriteLine("\n\nProcesed file:\n");
////Forms the markdown file name from the PDF name.
//var processedFileName = $"ProcessingTest.md";

//// Builds the full path to the markdown file.
//var processedFilePath = Path.Combine(processedDirectory, processedFileName);

//// Saves the markdown to disk.
//await File.WriteAllTextAsync(processedFilePath, testMarkdown ?? string.Empty);

//Console.WriteLine(testMarkdown);


/////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////Chank preparing/////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////


//var inputFolder = "InputDocuments";
//var outputFolder = "OutputChunks";

Directory.CreateDirectory(chunkDirectory);

var parser = new MarkdownChunkParser();
var writer = new JsonChunkWriter();

//foreach (var filePath in Directory.GetFiles(processedDirectory, "*.md"))
//{
//    var markdown = await File.ReadAllTextAsync(filePath);

//    var fileName = Path.GetFileNameWithoutExtension(filePath);

//    var documentId = fileName
//        .ToLowerInvariant()
//        .Replace(" ", "_");

//    var chunks = parser.Parse(
//        markdown: markdown,
//        documentId: documentId,
//        sourceFile: Path.GetFileName(filePath),
//        language: "ro"
//    );

//    var outputPath = Path.Combine(chunkDirectory, $"{documentId}.chunks.json");

//    await writer.WriteAsync(outputPath, chunks);

//    Console.WriteLine($"{fileName}: {chunks.Count} chunks");
//}

var markdownToProcess = await File.ReadAllTextAsync(Path.Combine(processedDirectory, "ProcessingTest.md"));

var fileName = Path.GetFileNameWithoutExtension(Path.Combine(processedDirectory, "ProcessingTest.md"));

var documentId = fileName
    .ToLowerInvariant()
    .Replace(" ", "_");

var chunks = parser.Parse(
    markdown: markdownToProcess,
    documentId: documentId,
    sourceFile: Path.GetFileName(Path.Combine(processedDirectory, "ProcessingTest.md")),
    language: "ro"
);

var outputPath = Path.Combine(chunkDirectory, $"{documentId}.chunks.json");

await writer.WriteAsync(outputPath, chunks);

Console.WriteLine($"{fileName}: {chunks.Count} chunks");