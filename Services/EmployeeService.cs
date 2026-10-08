using System.Data;
using cse325_group_project.Data;
using cse325_group_project.Models;
using Microsoft.Data.SqlClient;

namespace cse325_group_project.Services;

// Service contract for reading employee records from the database.
public interface IEmployeeService
{
    // Retrieves every employee record, sorted by last name then first name.
    Task<IReadOnlyList<Employee>> GetAllEmployeesAsync();
}

// Service responsible for reading employee records from the Azure SQL database.
public class EmployeeService : IEmployeeService
{
    // NOTE: table and column names are assumed - confirm against the real [dbo].[employee] table.
    private const string GetAllQuery = @"
        SELECT employee_id, first_name, last_name, email, department
        FROM dbo.employee
        ORDER BY last_name, first_name";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<EmployeeService> _logger;

    // Initializes a new instance of the EmployeeService class.
    public EmployeeService(IDbConnectionFactory connectionFactory, ILogger<EmployeeService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    // Executes a query to fetch all employee records from [dbo].[employee].
    public async Task<IReadOnlyList<Employee>> GetAllEmployeesAsync()
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(GetAllQuery, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var employees = new List<Employee>();
        while (await reader.ReadAsync())
        {
            employees.Add(MapEmployee(reader));
        }

        _logger.LogInformation("Loaded {Count} employee records.", employees.Count);
        return employees;
    }

    // Converts one database row into an Employee. Optional columns may be NULL.
    public static Employee MapEmployee(IDataRecord record)
    {
        return new Employee
        {
            EmployeeId = record.GetInt32(record.GetOrdinal("employee_id")),
            FirstName = record.GetString(record.GetOrdinal("first_name")),
            LastName = record.GetString(record.GetOrdinal("last_name")),
            Email = record.GetString(record.GetOrdinal("email")),
            Department = GetNullableString(record, "department")
        };
    }

    // Reads a string column that may be NULL in the database.
    private static string? GetNullableString(IDataRecord record, string column)
    {
        int ordinal = record.GetOrdinal(column);
        return record.IsDBNull(ordinal) ? null : record.GetString(ordinal);
    }
}
