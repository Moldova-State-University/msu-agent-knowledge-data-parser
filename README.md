# LlamaParserV2

## Short Description

Console .NET application for batch processing PDF documents through LlamaParse and saving the result as Markdown. The project scans the `Storage/Raw` folder, skips files that were already processed using SHA-256, and writes the generated `.md` files to `Storage/Markdown`.

## Features

- Recursively finds PDF files in `Storage/Raw`.
- Sends new PDF files to LlamaParse through the HTTP API.
- Waits for the parsing job to finish and gets the final Markdown.
- Saves Markdown files to `Storage/Markdown`.
- Keeps a local cache of processed files in `Storage/State/processed-files.json`.
- Uses a custom parsing configuration: `tier=cost_effective`, `version=latest`, OCR for `ro`, Markdown tables, and a custom prompt for cleaning and normalizing document structure.

## Architecture

Processing flow:

1. `Program.cs` resolves the project root and the `Storage/Raw`, `Storage/Markdown`, and `Storage/State` directories.
2. `ProcessedFilesStore` loads the JSON cache of previously processed files.
3. For each PDF, the application calculates a SHA-256 hash through `FileHashHelper`.
4. If the hash is already in the cache, the file is skipped.
5. If the file is not in the cache, `LlamaParseClient` uploads the PDF to LlamaParse and starts a parsing job.
6. The application polls the job status until it reaches a final state.
7. Markdown is combined page by page and written to `Storage/Markdown/<file_name>.md`.
8. A new record with file name, relative path, hash, size, and processing time is added to `processed-files.json`.

## Technology Stack

- .NET SDK 10 / Target Framework `net10.0`
- C#
- `HttpClient` for LlamaParse API calls
- `System.Text.Json` for configuration and cache serialization
- LlamaParse API (`https://api.cloud.llamaindex.ai/api/v2/parse`)

## Project Structure

```text
.
├── Program.cs
├── LlamaParseClient.cs
├── LlamaParseHelper.cs
├── FileHashHelper.cs
├── ProcessedFileRecord.cs
├── ProcessedFilesStore.cs
├── LlamaParserV2.csproj
└── Storage
    ├── Raw
    │   └── *.pdf
    ├── Markdown
    │   └── *.md
    └── State
        └── processed-files.json
```

## Requirements

- .NET SDK 10.x
- Internet access to the LlamaParse API for processing new files
- PDF files in the `Storage/Raw` directory

## Configuration

The project does not use `.env`, `appsettings.json`, or Docker configuration.

### Actual configuration is defined in code

- parsing and OCR settings are defined in `Program.cs`;
- the base API URL is defined in `LlamaParseClient.cs`;
- LlamaParse authorization is currently hardcoded in `LlamaParseClient.cs` instead of being moved to external configuration.

## Installation and Run

```bash
git clone <repo-url>
cd LlamaParserV2
dotnet build
dotnet run
```

### What happens on run

- creates `Storage/Raw` and `Storage/Markdown` if they do not exist;
- reads the list of already processed files from `Storage/State/processed-files.json`;
- processes only new PDF files;
- saves Markdown output to `Storage/Markdown`.

## Usage Example

Put PDF files into `Storage/Raw`, then run:

```bash
dotnet run
```

### Expected result

- new `.md` files appear in `Storage/Markdown`;
- `Storage/State/processed-files.json` is updated with processed document records;
- on the next run, already processed PDF files are skipped.

## Planned Improvements

- The API key is currently hardcoded in the source code instead of being stored in environment variables or config.
- Add an `.env.example` or `appsettings.json` template.
- Add `global.json` to pin the SDK version at the repository level.
- Add tests and a separate first-run setup guide for new keys or environments.
