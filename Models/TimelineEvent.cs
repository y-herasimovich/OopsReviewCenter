namespace OopsReviewCenter.Models;

public class TimelineEvent
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Author { get; set; }
    
    public Incident Incident { get; set; } = null!;
}
