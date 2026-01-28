namespace OopsReviewCenter.Models;

public class Template
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "Incident"; // Incident, ActionItem, Timeline
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
