# Laddro.Career

C# SDK for the [Laddro Career API](https://api.laddro.com/reference).

## Install

```bash
dotnet add package Laddro.Career
```

## Usage

```csharp
using Laddro.Career;

var client = new LaddroClient("laddro_live_...");

// List resumes
var resumes = await client.ListResumesAsync();
foreach (var r in resumes.Items)
    Console.WriteLine(r.Title);

// Tailor a resume
var pdf = await client.TailorAsync(new TailorRequest
{
    PositionName = "Senior Frontend Engineer",
    JobUrl = "https://jobs.example.com/sfe"
});
File.WriteAllBytes("tailored.pdf", pdf);

// Generate cover letter
var cl = await client.GenerateCoverLetterAsync(new GenerateCoverLetterRequest
{
    PositionName = "Product Manager",
    JobUrl = "https://jobs.example.com/pm"
});

// Browse templates
var templates = await client.ListTemplatesAsync();

// BYOK
await client.UpdateAiSettingsAsync(new UpdateAISettingsRequest
{
    Provider = "Anthropic",
    Model = "claude-sonnet-4-20250514",
    ApiKey = "sk-ant-..."
});
```

## Links

- [laddro.com](https://laddro.com)
- [API Reference](https://api.laddro.com/reference)
- [Docs](https://docs.laddro.com)
- [GitHub](https://github.com/laddro-app)

## License

MIT
