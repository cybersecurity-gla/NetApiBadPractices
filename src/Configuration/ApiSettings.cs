namespace BadApiExample.Configuration;

public class ApiSettings
{
    public const string SectionName = "ApiSettings";
    
    public int DefaultPageSize { get; set; } = 10;
    public int MaxPageSize { get; set; } = 100;
    public int CacheExpirationMinutes { get; set; } = 30;
    public bool EnableSwaggerInProduction { get; set; } = false;
    public bool EnableDetailedErrors { get; set; } = false;
}