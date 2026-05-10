using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Laddro.Career;

public class LaddroClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public LaddroClient(string apiKey, string baseUrl = "https://api.laddro.com")
    {
        _apiKey = apiKey;
        _baseUrl = baseUrl.TrimEnd('/');
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
    }

    public async Task<List<Template>> ListTemplatesAsync()
    {
        var resp = await GetAsync<TemplateListResponse>("/v1/templates");
        return resp.Templates;
    }

    public Task<TemplateDetail> GetTemplateAsync(string templateId) =>
        GetAsync<TemplateDetail>($"/v1/templates/{templateId}");

    public async Task<List<TemplateFont>> ListFontsAsync()
    {
        var resp = await GetAsync<FontListResponse>("/v1/fonts");
        return resp.Fonts;
    }

    public async Task<List<Language>> ListLanguagesAsync()
    {
        var resp = await GetAsync<LanguageListResponse>("/v1/languages");
        return resp.Languages;
    }

    public async Task<List<ModelProvider>> ListModelsAsync()
    {
        var resp = await GetAsync<ModelListResponse>("/v1/models");
        return resp.Models;
    }

    public Task<PaginatedList<ResumeSummary>> ListResumesAsync(int limit = 20, int offset = 0) =>
        GetAsync<PaginatedList<ResumeSummary>>($"/v1/resumes?limit={limit}&offset={offset}");

    public Task<ResumeSummary> GetResumeAsync(string resumeId) =>
        GetAsync<ResumeSummary>($"/v1/resumes/{resumeId}");

    public Task<byte[]> RenderResumeAsync(string resumeId, RenderOptions options) =>
        PutBinaryAsync($"/v1/resumes/{resumeId}/render", options);

    public Task<byte[]> TailorAsync(TailorRequest request) =>
        PostBinaryAsync("/v1/tailor", request);

    public Task<byte[]> ExportPdfAsync(ExportRequest request) =>
        PostBinaryAsync("/v1/export", request);

    public Task<PaginatedList<CoverLetterSummary>> ListCoverLettersAsync(int limit = 20, int offset = 0) =>
        GetAsync<PaginatedList<CoverLetterSummary>>($"/v1/cover-letters?limit={limit}&offset={offset}");

    public Task<CoverLetterSummary> GetCoverLetterAsync(string id) =>
        GetAsync<CoverLetterSummary>($"/v1/cover-letters/{id}");

    public Task<CreateCoverLetterResponse> CreateCoverLetterAsync(CreateCoverLetterRequest request) =>
        PostAsync<CreateCoverLetterResponse>("/v1/cover-letters", request);

    public Task<byte[]> GenerateCoverLetterAsync(GenerateCoverLetterRequest request) =>
        PostBinaryAsync("/v1/cover-letters/generate", request);

    public Task<byte[]> RenderCoverLetterAsync(string id, RenderOptions options) =>
        PutBinaryAsync($"/v1/cover-letters/{id}/render", options);

    public Task<SettingsResponse> GetSettingsAsync() =>
        GetAsync<SettingsResponse>("/v1/settings");

    public Task<SettingsResponse> UpdateAiSettingsAsync(UpdateAISettingsRequest request) =>
        PutAsync<SettingsResponse>("/v1/settings/model", request);

    public Task<SettingsResponse> DeleteAiSettingsAsync() =>
        DeleteAsync<SettingsResponse>("/v1/settings/model");

    private async Task<T> GetAsync<T>(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _baseUrl + path);
        AddAuth(request);
        var response = await _http.SendAsync(request);
        await EnsureSuccess(response);
        return (await response.Content.ReadFromJsonAsync<T>(_json))!;
    }

    private async Task<T> PostAsync<T>(string path, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + path)
        {
            Content = JsonContent.Create(body, options: _json)
        };
        AddAuth(request);
        var response = await _http.SendAsync(request);
        await EnsureSuccess(response);
        return (await response.Content.ReadFromJsonAsync<T>(_json))!;
    }

    private async Task<byte[]> PostBinaryAsync(string path, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + path)
        {
            Content = JsonContent.Create(body, options: _json)
        };
        AddAuth(request);
        var response = await _http.SendAsync(request);
        await EnsureSuccess(response);
        return await response.Content.ReadAsByteArrayAsync();
    }

    private async Task<T> PutAsync<T>(string path, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, _baseUrl + path)
        {
            Content = JsonContent.Create(body, options: _json)
        };
        AddAuth(request);
        var response = await _http.SendAsync(request);
        await EnsureSuccess(response);
        return (await response.Content.ReadFromJsonAsync<T>(_json))!;
    }

    private async Task<byte[]> PutBinaryAsync(string path, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, _baseUrl + path)
        {
            Content = JsonContent.Create(body, options: _json)
        };
        AddAuth(request);
        var response = await _http.SendAsync(request);
        await EnsureSuccess(response);
        return await response.Content.ReadAsByteArrayAsync();
    }

    private async Task<T> DeleteAsync<T>(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, _baseUrl + path);
        AddAuth(request);
        var response = await _http.SendAsync(request);
        await EnsureSuccess(response);
        return (await response.Content.ReadFromJsonAsync<T>(_json))!;
    }

    private void AddAuth(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(_apiKey))
            request.Headers.Add("x-api-key", _apiKey);
    }

    private async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync();
        string message;
        string? code = null;
        try
        {
            var err = JsonSerializer.Deserialize<ErrorBody>(body, _json);
            message = err?.Error ?? body;
            code = err?.Code;
        }
        catch { message = body; }
        throw new LaddroException(message, (int)response.StatusCode, code);
    }

    private record ErrorBody(string? Error, string? Code);
}
