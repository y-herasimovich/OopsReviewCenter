namespace OopsReviewCenter.Models;

public class ActionItem
{
    public int Id { get; set; }
    public int? IncidentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Open"; // Open, In Progress, Completed, Cancelled
    public string Priority { get; set; } = "Medium"; // Low, Medium, High
    public string? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public Incident? Incident { get; set; }
}
