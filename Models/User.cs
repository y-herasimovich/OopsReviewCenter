using System.ComponentModel.DataAnnotations;

namespace OopsReviewCenter.Models;

public class User
{
    public int UserId { get; set; }
    
    public int RoleId { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Salt { get; set; } = string.Empty;
    
    public bool? IsActive { get; set; }
    
    [MaxLength(200)]
    public string? Username { get; set; }
    
    [MaxLength(200)]
    public string? Email { get; set; }
    
    [MaxLength(200)]
    public string? FullName { get; set; }
    
    public Role Role { get; set; } = null!;
    
    public List<Incident> ResolvedIncidents { get; set; } = new();
}
