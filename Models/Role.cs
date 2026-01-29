using System.ComponentModel.DataAnnotations;

namespace OopsReviewCenter.Models;

public class Role
{
    public int RoleId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public List<User> Users { get; set; } = new();
}
