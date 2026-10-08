using System.ComponentModel.DataAnnotations;

namespace cse325_group_project.Models;

// Represents HR account data used by
// Create Account and Edit Account forms.
public class HrProfileFormModel
{
    [Required(ErrorMessage = "The HR ID field is required.")]
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "The HR ID field must be a number greater than zero.")]
    
    // Unique HR employee identifier.
    public int HrId { get; set; }

    [Required(ErrorMessage = "The First Name field is required.")]
    [StringLength(50)]

    // HR employee's first name.
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "The Last Name field is required.")]
    [StringLength(50)]

    // HR employee's last name.
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "The Email field is required.")]
    [EmailAddress]

    // HR employee's email address.
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "The Password field is required.")]
    [MinLength(
        8,
        ErrorMessage = "The Password field must be with a minimum length of '8'.")]
    
    // Password entered by the user before hashing.
    public string Password { get; set; } = string.Empty;
}