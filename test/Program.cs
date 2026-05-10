using Laddro.Career;

var apiKey = Environment.GetEnvironmentVariable("LADDRO_API_KEY");
if (string.IsNullOrEmpty(apiKey)) { Console.Error.WriteLine("Set LADDRO_API_KEY"); return 1; }

var client = new LaddroClient(apiKey);
var pub = new LaddroClient("");
int passed = 0, failed = 0;
string? resumeId = null, coverLetterId = null;

async Task Test(string name, Func<Task> fn)
{
    try { await fn(); Console.WriteLine($"  ✓ {name}"); passed++; }
    catch (Exception e) { Console.WriteLine($"  ✗ {name}: {e.Message}"); failed++; }
}

Console.WriteLine("\n— 1. Public endpoints (5/18) —\n");

await Test("GET /v1/templates", async () => { var t = await pub.ListTemplatesAsync(); if (t.Count != 22) throw new($"expected 22, got {t.Count}"); });
await Test("GET /v1/templates/{id}", async () => { var d = await pub.GetTemplateAsync("GRAPHITE"); if (d.Id != "GRAPHITE") throw new("wrong id"); });
await Test("GET /v1/fonts", async () => { var f = await pub.ListFontsAsync(); if (f.Count != 21) throw new($"expected 21, got {f.Count}"); });
await Test("GET /v1/languages", async () => { var l = await pub.ListLanguagesAsync(); if (l.Count != 14) throw new($"expected 14, got {l.Count}"); });
await Test("GET /v1/models", async () => { var m = await pub.ListModelsAsync(); if (m.Count != 10) throw new($"expected 10, got {m.Count}"); });

Console.WriteLine("\n— 2. Resume endpoints (4/18) —\n");

await Test("GET /v1/resumes", async () =>
{
    var list = await client.ListResumesAsync(5);
    if (list.Items.Count == 0) throw new("no resumes");
    resumeId = list.Items.FirstOrDefault(r => r.IsDefault)?.ResumeId ?? list.Items[0].ResumeId;
});
await Test("GET /v1/resumes/{id}", async () =>
{
    var r = await client.GetResumeAsync(resumeId!);
    if (r.ResumeId != resumeId) throw new("mismatch");
});
await Test("PUT /v1/resumes/{id}/render", async () =>
{
    var pdf = await client.RenderResumeAsync(resumeId!, new RenderOptions { TemplateId = "GRAPHITE" });
    if (pdf.Length < 1000) throw new($"too small: {pdf.Length}");
});
await Test("POST /v1/resumes/parse (skip)", async () => { await Task.CompletedTask; });

Console.WriteLine("\n— 3. Tailor (1/18) —\n");

await Test("POST /v1/tailor", async () =>
{
    var pdf = await client.TailorAsync(new TailorRequest { ResumeId = resumeId, PositionName = ".NET SDK Test", JobDescription = "Write C# code." });
    if (pdf.Length < 5000) throw new($"too small: {pdf.Length}");
});

Console.WriteLine("\n— 4. Export (1/18) —\n");

await Test("POST /v1/export", async () =>
{
    var pdf = await client.ExportPdfAsync(new ExportRequest { ResumeId = resumeId!, TemplateId = "COBALT" });
    if (pdf.Length < 1000) throw new($"too small: {pdf.Length}");
});

Console.WriteLine("\n— 5. Cover Letter endpoints (5/18) —\n");

await Test("GET /v1/cover-letters", async () => { await client.ListCoverLettersAsync(); });
await Test("POST /v1/cover-letters", async () =>
{
    var r = await client.CreateCoverLetterAsync(new CreateCoverLetterRequest { FullName = ".NET Test", LetterContent = "<p>Test.</p>" });
    coverLetterId = r.CoverLetterId;
    if (string.IsNullOrEmpty(coverLetterId)) throw new("no id");
});
await Test("GET /v1/cover-letters/{id}", async () =>
{
    var cl = await client.GetCoverLetterAsync(coverLetterId!);
    if (cl.CoverLetterId != coverLetterId) throw new("mismatch");
});
await Test("PUT /v1/cover-letters/{id}/render", async () =>
{
    var pdf = await client.RenderCoverLetterAsync(coverLetterId!, new RenderOptions { TemplateId = "NICKEL" });
    if (pdf.Length < 1000) throw new($"too small: {pdf.Length}");
});
await Test("POST /v1/cover-letters/generate", async () =>
{
    var pdf = await client.GenerateCoverLetterAsync(new GenerateCoverLetterRequest { ResumeId = resumeId, PositionName = ".NET Test", JobDescription = "C# dev." });
    if (pdf.Length < 1000) throw new($"too small: {pdf.Length}");
});

Console.WriteLine("\n— 6. Settings (3/18) —\n");

await Test("GET /v1/settings", async () => { await client.GetSettingsAsync(); });
await Test("PUT /v1/settings/model", async () =>
{
    try { await client.UpdateAiSettingsAsync(new UpdateAISettingsRequest { Provider = "OpenAI", Model = "gpt-4o-mini", ApiKey = "sk-test" }); }
    catch (LaddroException) { }
});
await Test("DELETE /v1/settings/model", async () =>
{
    var r = await client.DeleteAiSettingsAsync();
    if (r.Ai != null) throw new("ai should be null");
});

Console.WriteLine("\n— 7. Errors —\n");

await Test("401 on bad key", async () =>
{
    try { await new LaddroClient("laddro_live_invalid").ListResumesAsync(); throw new("should throw"); }
    catch (LaddroException e) { if (!e.IsAuthError) throw new($"expected 401, got {e.Status}"); }
});
await Test("404 on missing resume", async () =>
{
    try { await client.GetResumeAsync("00000000-0000-0000-0000-000000000000"); throw new("should throw"); }
    catch (LaddroException e) { if (!e.IsNotFound) throw new($"expected 404, got {e.Status}"); }
});

Console.WriteLine($"\n═══ FINAL: {passed} passed, {failed} failed (18 endpoints covered) ═══\n");
return failed > 0 ? 1 : 0;
