namespace Laddro.Career;

public record ArtifactMetadata(string? ResumeId, string? CoverLetterId, string? Filename, string? MimeType);
public record BinaryResponse(byte[] Data, ArtifactMetadata Metadata);
public record ResumeSummary(string Id, string ResumeId, string Title, bool IsDefault, string CreatedAt, string UpdatedAt);
public record PaginatedList<T>(List<T> Items, int Total, int Limit, int Offset);
public record Template(string Id, string Name, int AtsScore, string LayoutType, bool SupportsProfileImage);
public record TemplateFont(string Family, string Label);
public record TemplateColor(string Id, string BackgroundColor, string? BackgroundPartColor, string? UnderlineColor);
public record TemplateDetail(string Id, string Name, int AtsScore, string LayoutType, bool SupportsProfileImage, List<TemplateColor> AvailableColors, List<TemplateFont> AvailableFonts);
public record Language(string Code, string Name);
public record Model(string Id, string Name, bool Recommended);
public record ModelProvider(string Provider, string Name, string BaseUrl, List<Model> Models, string KeyPrefix, string DocsUrl);
public record CoverLetterSummary(string Id, string CoverLetterId, string Title, string CreatedAt, string UpdatedAt, string? LetterContent, Dictionary<string, object>? Data);
public record CreateCoverLetterResponse(string CoverLetterId, string Title, string Status);
public record AISettings(string Provider, string Model, string BaseUrl, bool HasApiKey, string? UpdatedAt);
public record SettingsResponse(AISettings? Ai);

public class RenderOptions
{
    public string TemplateId { get; set; } = "";
    public string? Locale { get; set; }
    public string? ColorId { get; set; }
    public string? Font { get; set; }
    public double? Spacing { get; set; }
    public double? Margin { get; set; }
    public double? FontSize { get; set; }
    public string? PageNumbering { get; set; }
}

public class TailorRequest
{
    public string? ResumeId { get; set; }
    public string PositionName { get; set; } = "";
    public string? JobDescription { get; set; }
    public string? JobUrl { get; set; }
    public string? Mode { get; set; }
    public string? Language { get; set; }
    public bool? IncludeCoverLetter { get; set; }
    public string? TemplateId { get; set; }
    public string? ColorId { get; set; }
    public string? Font { get; set; }
}

public class ExportRequest
{
    public string ResumeId { get; set; } = "";
    public string? TemplateId { get; set; }
    public string? Locale { get; set; }
    public string? ColorId { get; set; }
    public string? Font { get; set; }
    public double? Spacing { get; set; }
    public double? Margin { get; set; }
    public double? FontSize { get; set; }
    public string? PageNumbering { get; set; }
}

public class CreateCoverLetterRequest
{
    public string? Title { get; set; }
    public string FullName { get; set; } = "";
    public string? JobTitle { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? CompanyName { get; set; }
    public string? HiringManager { get; set; }
    public string LetterContent { get; set; } = "";
}

public class GenerateCoverLetterRequest
{
    public string? ResumeId { get; set; }
    public string PositionName { get; set; } = "";
    public string? JobDescription { get; set; }
    public string? JobUrl { get; set; }
    public string? Language { get; set; }
    public string? TemplateId { get; set; }
}

public class UpdateAISettingsRequest
{
    public string Provider { get; set; } = "";
    public string? Model { get; set; }
    public string ApiKey { get; set; } = "";
}

internal record TemplateListResponse(List<Template> Templates);
internal record FontListResponse(List<TemplateFont> Fonts);
internal record LanguageListResponse(List<Language> Languages);
internal record ModelListResponse(List<ModelProvider> Models);
