using System.Data;
using cse325_group_project.Services;

namespace cse325_group_project.Tests.Services;

// Tests that a database row is turned into an Employee correctly, without a real database.
public class EmployeeServiceTests
{
    private static IDataReader CreateReader(params object[] row)
    {
        var table = new DataTable();
        table.Columns.Add("employee_id", typeof(int));
        table.Columns.Add("first_name", typeof(string));
        table.Columns.Add("last_name", typeof(string));
        table.Columns.Add("email", typeof(string));
        table.Columns.Add("position", typeof(string));
        table.Columns.Add("department", typeof(string));
        table.Columns.Add("status", typeof(string));
        table.Columns.Add("start_date", typeof(DateTime));
        table.Columns.Add("responsibilities", typeof(string));
        table.Rows.Add(row);

        var reader = table.CreateDataReader();
        Assert.True(reader.Read());
        return reader;
    }

    [Fact]
    public void MapEmployee_MapsAllColumns()
    {
        var reader = CreateReader(7, "Ada", "Lovelace", "ada@ems.com", "Engineer", "IT",
            "Active", new DateTime(2024, 3, 15), "Builds the analytical engine");

        var employee = EmployeeService.MapEmployee(reader);

        Assert.Equal(7, employee.EmployeeId);
        Assert.Equal("Ada", employee.FirstName);
        Assert.Equal("Lovelace", employee.LastName);
        Assert.Equal("ada@ems.com", employee.Email);
        Assert.Equal("Engineer", employee.Position);
        Assert.Equal("IT", employee.Department);
        Assert.Equal("Active", employee.Status);
        Assert.Equal(new DateTime(2024, 3, 15), employee.StartDate);
        Assert.Equal("Builds the analytical engine", employee.Responsibilities);
        Assert.Equal("Ada Lovelace", employee.FullName);
    }
}
