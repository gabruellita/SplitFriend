namespace FinanceService.Infrastructure.Models;

public class Category
{
    public long     Id              { get; set; }
    public string   Name            { get; set; } = string.Empty;
    public string   Kind            { get; set; } = string.Empty;   // INCOME / EXPENSE
    public string?  Icon            { get; set; }
    public string?  Color           { get; set; }
    public long?    CreatedByUserId { get; set; }
    public bool     IsSystem        { get; set; }
    public bool     IsActive        { get; set; }
    public DateTime CreatedAt       { get; set; }
    public DateTime UpdatedAt       { get; set; }
}
