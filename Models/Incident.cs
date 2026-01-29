namespace OopsReviewCenter.Models;

public class Incident
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string Status { get; set; } = "Open"; // Open, Investigating, Resolved, Closed
    public string? RootCause { get; set; }
    public string? Impact { get; set; }
    
    public List<TimelineEvent> TimelineEvents { get; set; } = new();
    public List<ActionItem> ActionItems { get; set; } = new();
    public List<IncidentTag> IncidentTags { get; set; } = new();
}
