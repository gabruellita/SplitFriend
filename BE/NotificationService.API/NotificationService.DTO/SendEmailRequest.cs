namespace NotificationService.DTO;

/// <summary>
/// Cerere generica: `Template` alege continutul; `Data` aduce valorile (ex. groupName, link).
/// </summary>
public record SendEmailRequest(
    string                       To,
    string                       Template,   // ex. "group-invite"
    IReadOnlyDictionary<string,string> Data
);
