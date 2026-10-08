using System.ComponentModel.DataAnnotations;
using cse325_group_project.Models;

namespace cse325_group_project.Tests.Models;

// Tests the validation rules on the Create New Record form.
public class EmployeeFormModelTests
{
    private static EmployeeFormModel ValidModel() => new()
    {
        EmployeeId = 3,
        FirstName = "Grace",
        LastName = "Hopper",
        Email = "grace@ems.com",
        Position = "Frontend Engineer",
        Department = "Engineering",
        Status = "Active",
        StartDate = new DateTime(2026, 9, 1),
        Responsibilities = "Writes compilers"
    };

    // Returns the names of the fields that failed validation
    private static List<string> InvalidFields(EmployeeFormModel model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results.SelectMany(r => r.MemberNames).ToList();
    }

    [Fact]
    public void ValidModelHasNoErrors()
    {
        Assert.Empty(InvalidFields(ValidModel()));
    }

    [Fact]
    public void EmptyModelReportsEveryRequiredField()
    {
        var model = new EmployeeFormModel { StartDate = null };

        var invalid = InvalidFields(model);

        Assert.Equivalent(new[]
        {
            "EmployeeId", "FirstName", "LastName", "Email", "Position",
            "Department", "Status", "StartDate", "Responsibilities"
        }, invalid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void EmployeeIdMustBeGreaterThanZero(int id)
    {
        var model = ValidModel();
        model.EmployeeId = id;

        Assert.Equal(["EmployeeId"], InvalidFields(model));
    }

    [Fact]
    public void EmailMustBeAValidAddress()
    {
        var model = ValidModel();
        model.Email = "not-an-email";

        Assert.Equal(["Email"], InvalidFields(model));
    }

    [Fact]
    public void NamesLongerThan50CharactersAreRejected()
    {
        var model = ValidModel();
        model.FirstName = new string('a', 51);

        Assert.Equal(["FirstName"], InvalidFields(model));
    }

    [Fact]
    public void StatusLongerThan20CharactersIsRejected()
    {
        var model = ValidModel();
        model.Status = new string('a', 21);

        Assert.Equal(["Status"], InvalidFields(model));
    }

    [Fact]
    public void ToEmployeeTrimsSpacesAndCopiesEveryField()
    {
        var model = ValidModel();
        model.FirstName = "  Grace ";
        model.StartDate = new DateTime(2026, 9, 1, 14, 30, 0);

        var employee = model.ToEmployee();

        Assert.Equal(3, employee.EmployeeId);
        Assert.Equal("Grace", employee.FirstName);
        Assert.Equal("Hopper", employee.LastName);
        Assert.Equal("grace@ems.com", employee.Email);
        Assert.Equal("Frontend Engineer", employee.Position);
        Assert.Equal("Engineering", employee.Department);
        Assert.Equal("Active", employee.Status);
        Assert.Equal(new DateTime(2026, 9, 1), employee.StartDate);
        Assert.Equal("Writes compilers", employee.Responsibilities);
    }
}
