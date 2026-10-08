using System.Data;
using Microsoft.Data.SqlClient;
using cse325_group_project.Data;
using cse325_group_project.Models;

namespace cse325_group_project.Services;

public interface IEmployeeService
{
    Task<List<Employee>> GetAllEmployeesAsync();
}

public class EmployeeService : IEmployeeService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IDbConnectionFactory connectionFactory, ILogger<EmployeeService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        List<Employee> employees = new();

        await using var connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync();

        const string query = @"
            SELECT
                employee_id,
                first_name,
                last_name,
                email,
                position,
                department,
                status,
                start_date,
                responsibilities
            FROM dbo.employee
            ORDER BY employee_id";

        await using var command =
            new SqlCommand(query, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            employees.Add(MapEmployee(reader));
        }

        _logger.LogInformation("Loaded {Count} employee records.", employees.Count);
        return employees;
    }

    // Converts one database row into an Employee (public so it can be tested without a database)
    public static Employee MapEmployee(IDataRecord reader)
    {
        return new Employee
        {
            EmployeeId =
                reader.GetInt32(reader.GetOrdinal("employee_id")),
            FirstName =
                reader.GetString(reader.GetOrdinal("first_name")),
            LastName =
                reader.GetString(reader.GetOrdinal("last_name")),
            Email =
                reader.GetString(reader.GetOrdinal("email")),
            Position =
                reader.GetString(reader.GetOrdinal("position")),
            Department =
                reader.GetString(reader.GetOrdinal("department")),
            Status =
                reader.GetString(reader.GetOrdinal("status")),
            StartDate =
                reader.GetDateTime(reader.GetOrdinal("start_date")),
            Responsibilities =
                reader.GetString(reader.GetOrdinal("responsibilities"))
        };
    }
}
