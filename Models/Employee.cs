namespace cse325_group_project.Models;

// Represents an employee record mapped from the database [dbo].[employee] table.
// NOTE: column names are assumed (see EmployeeService) - confirm against the real table.
public class Employee
{
    // Unique identifier for the employee.
    public int EmployeeId { get; set; }

    // First name of the employee.
    public string FirstName { get; set; } = string.Empty;

    // Last name of the employee.
    public string LastName { get; set; } = string.Empty;

    // Work email address.
    public string Email { get; set; } = string.Empty;

    // Department name, if any.
    public string? Department { get; set; }

    // First and last name together, for display.
    public string FullName => $"{FirstName} {LastName}".Trim();
}
