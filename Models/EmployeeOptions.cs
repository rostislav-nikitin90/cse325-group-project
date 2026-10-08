namespace cse325_group_project.Models;

// Choices for the Create New Record dropdowns.
// The database has CHECK rules on these columns and rejects any other value,
// so these lists must match those rules. They were taken from the values
// existing employees already use - add new ones here if the database allows more.
public static class EmployeeOptions
{
    public static readonly string[] Positions =
    [
        "Brand Designer",
        "Finance Operation",
        "Frontend Engineer",
        "Head of People",
        "Product manager"
    ];

    public static readonly string[] Departments =
    [
        "Design",
        "Engineering",
        "Operations",
        "People",
        "Product"
    ];

    public static readonly string[] Statuses =
    [
        "Active",
        "On leave",
        "Onboarding"
    ];
}
