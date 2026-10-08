using System.ComponentModel.DataAnnotations;

namespace cse325_group_project.Models;

// Represents employee data entered in the Create New Record form.
public class EmployeeFormModel
{
    [Required(ErrorMessage = "The Employee ID field is required.")]
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "The Employee ID field must be a number greater than zero.")]

    // Unique employee identifier.
    public int? EmployeeId { get; set; }

    [Required(ErrorMessage = "The First Name field is required.")]
    [StringLength(50)]

    // Employee's first name.
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "The Last Name field is required.")]
    [StringLength(50)]

    // Employee's last name.
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "The Email field is required.")]
    [EmailAddress(ErrorMessage = "The Email field is not a valid email address.")]
    [StringLength(100)]

    // Employee's work email address.
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "The Position field is required.")]
    [StringLength(50)]

    // Employee's job position.
    public string Position { get; set; } = string.Empty;

    [Required(ErrorMessage = "The Department field is required.")]
    [StringLength(50)]

    // Department the employee works in.
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "The Status field is required.")]
    [StringLength(20)]

    // Employment status, e.g. Active.
    public string Status { get; set; } = string.Empty;

    [Required(ErrorMessage = "The Start Date field is required.")]

    // Date the employee started working.
    public DateTime? StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "The Responsibilities field is required.")]
    [StringLength(500)]

    // Description of the employee's responsibilities.
    public string Responsibilities { get; set; } = string.Empty;

    // Converts the form data into an Employee record (call only after validation passes).
    public Employee ToEmployee() => new()
    {
        EmployeeId = EmployeeId!.Value,
        FirstName = FirstName.Trim(),
        LastName = LastName.Trim(),
        Email = Email.Trim(),
        Position = Position.Trim(),
        Department = Department.Trim(),
        Status = Status.Trim(),
        StartDate = StartDate!.Value.Date,
        Responsibilities = Responsibilities.Trim()
    };
}
