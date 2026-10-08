using System.Data;
using cse325_group_project.Services;

namespace cse325_group_project.Tests.Services;

// Tests that a database row is turned into an Employee correctly, without a real database.
public class EmployeeServiceTests
{
    private static DataTable CreateEmployeeTable()
    {
        var table = new DataTable();
        table.Columns.Add("employee_id", typeof(int));
        table.Columns.Add("first_name", typeof(string));
        table.Columns.Add("last_name", typeof(string));
        table.Columns.Add("email", typeof(string));
        table.Columns.Add("department", typeof(string));
        return table;
    }

    private static IDataReader ReadFirstRow(DataTable table)
    {
        var reader = table.CreateDataReader();
        Assert.True(reader.Read());
        return reader;
    }

    [Fact]
    public void MapEmployee_MapsAllColumns()
    {
        var table = CreateEmployeeTable();
        table.Rows.Add(7, "Ada", "Lovelace", "ada@ems.com", "IT");

        var employee = EmployeeService.MapEmployee(ReadFirstRow(table));

        Assert.Equal(7, employee.EmployeeId);
        Assert.Equal("Ada", employee.FirstName);
        Assert.Equal("Lovelace", employee.LastName);
        Assert.Equal("ada@ems.com", employee.Email);
        Assert.Equal("IT", employee.Department);
        Assert.Equal("Ada Lovelace", employee.FullName);
    }

    [Fact]
    public void MapEmployee_NullDepartment_BecomesNull()
    {
        var table = CreateEmployeeTable();
        table.Rows.Add(8, "Alan", "Turing", "alan@ems.com", DBNull.Value);

        var employee = EmployeeService.MapEmployee(ReadFirstRow(table));

        Assert.Null(employee.Department);
    }
}
