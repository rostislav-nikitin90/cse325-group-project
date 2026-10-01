namespace cse325_group_project.Models;

// Represents an HR user account entity mapped from the database [dbo].[hr] table.
public class HrUser
{
    // Unique identifier for the HR user.
    public int HrId { get; set; }

    // First name of the HR user.
    public string FirstName { get; set; } = string.Empty;

    // Last name of the HR user.
    public string LastName { get; set; } = string.Empty;

    // Work email address used for login and notifications.
    public string Email { get; set; } = string.Empty;

    // BCrypt cryptographic hash of the user's password.
    public string PasswordHash { get; set; } = string.Empty;
}

// Data transfer model capturing login form inputs from the user interface.
public class LoginModel
{
    // Email address provided during login.
    public string Email { get; set; } = string.Empty;

    // Plaintext password entered by the user.
    public string Password { get; set; } = string.Empty;
}
