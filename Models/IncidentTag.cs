namespace OopsReviewCenter.Models;

public class IncidentTag
{
    public int IncidentId { get; set; }
    public int TagId { get; set; }
    
    public Incident Incident { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
