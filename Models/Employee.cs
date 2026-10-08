namespace cse325_group_project.Models;

public class Employee
{
    public int EmployeeId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public string Responsibilities { get; set; } = string.Empty;

    // First and last name together, used in button labels for screen readers
    public string FullName => $"{FirstName} {LastName}".Trim();
}
