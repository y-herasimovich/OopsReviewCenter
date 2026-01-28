namespace OopsReviewCenter.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public List<IncidentTag> IncidentTags { get; set; } = new();
}
