using System.Data;
using Microsoft.Data.SqlClient;
using cse325_group_project.Data;
using cse325_group_project.Models;

namespace cse325_group_project.Services;

public interface IEmployeeService
{
    Task<List<Employee>> GetAllEmployeesAsync();

    // Inserts a new employee record.
    // Throws DuplicateEmployeeException if the ID or email is already used,
    // or InvalidEmployeeValueException if the database rejects a value.
    Task CreateEmployeeAsync(Employee employee);

    Task<Employee?> GetEmployeeByIdAsync(int employeeId);

    Task UpdateEmployeeAsync(Employee employee);
}

// Thrown when the database's CHECK rules reject a value (position, department or status).
public class InvalidEmployeeValueException : Exception
{
    public InvalidEmployeeValueException(Exception innerException)
        : base("The database does not allow one of the employee's values.", innerException)
    {
    }
}

// Thrown when a new employee's ID or email already exists in the database.
public class DuplicateEmployeeException : Exception
{
    public DuplicateEmployeeException(Exception innerException)
        : base("An employee with this ID or email already exists.", innerException)
    {
    }
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

    public async Task CreateEmployeeAsync(Employee employee)
    {
        await using var connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync();

        const string query = @"
            INSERT INTO dbo.employee
            (
                employee_id,
                first_name,
                last_name,
                email,
                position,
                department,
                status,
                start_date,
                responsibilities
            )
            VALUES
            (
                @EmployeeId,
                @FirstName,
                @LastName,
                @Email,
                @Position,
                @Department,
                @Status,
                @StartDate,
                @Responsibilities
            )";

        await using var command =
            new SqlCommand(query, connection);

        // Sizes match the limits in EmployeeFormModel, so values are never cut off
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employee.EmployeeId;
        command.Parameters.Add("@FirstName", SqlDbType.VarChar, 50).Value = employee.FirstName;
        command.Parameters.Add("@LastName", SqlDbType.VarChar, 50).Value = employee.LastName;
        command.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = employee.Email;
        command.Parameters.Add("@Position", SqlDbType.VarChar, 50).Value = employee.Position;
        command.Parameters.Add("@Department", SqlDbType.VarChar, 50).Value = employee.Department;
        command.Parameters.Add("@Status", SqlDbType.VarChar, 20).Value = employee.Status;
        command.Parameters.Add("@StartDate", SqlDbType.Date).Value = employee.StartDate;
        command.Parameters.Add("@Responsibilities", SqlDbType.VarChar, 500).Value = employee.Responsibilities;

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        // 2627 = primary key / unique constraint, 2601 = unique index
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new DuplicateEmployeeException(ex);
        }
        // 547 = CHECK constraint (only certain positions, departments and statuses are allowed)
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new InvalidEmployeeValueException(ex);
        }

        _logger.LogInformation("Created employee {EmployeeId}.", employee.EmployeeId);
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

    public async Task<Employee?> GetEmployeeByIdAsync(
        int employeeId)
    {
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
            WHERE employee_id = @EmployeeId";

        await using var command =
            new SqlCommand(query, connection);

        command.Parameters.Add(
            "@EmployeeId",
            SqlDbType.Int).Value = employeeId;

        await using var reader =
            await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapEmployee(reader);
        }

        return null;
    }

    public async Task UpdateEmployeeAsync(
        Employee employee)
    {
        await using var connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync();

        const string query = @"
            UPDATE dbo.employee
            SET
                first_name = @FirstName,
                last_name = @LastName,
                email = @Email,
                position = @Position,
                department = @Department,
                status = @Status,
                start_date = @StartDate,
                responsibilities = @Responsibilities
            WHERE employee_id = @EmployeeId";

        await using var command =
            new SqlCommand(query, connection);

        command.Parameters.Add("@EmployeeId",
            SqlDbType.Int).Value =
            employee.EmployeeId;

        command.Parameters.Add("@FirstName",
            SqlDbType.VarChar, 50).Value =
            employee.FirstName;

        command.Parameters.Add("@LastName",
            SqlDbType.VarChar, 50).Value =
            employee.LastName;

        command.Parameters.Add("@Email",
            SqlDbType.VarChar, 100).Value =
            employee.Email;

        command.Parameters.Add("@Position",
            SqlDbType.VarChar, 50).Value =
            employee.Position;

        command.Parameters.Add("@Department",
            SqlDbType.VarChar, 50).Value =
            employee.Department;

        command.Parameters.Add("@Status",
            SqlDbType.VarChar, 20).Value =
            employee.Status;

        command.Parameters.Add("@StartDate",
            SqlDbType.Date).Value =
            employee.StartDate;

        command.Parameters.Add("@Responsibilities",
            SqlDbType.VarChar, 500).Value =
            employee.Responsibilities;

        await command.ExecuteNonQueryAsync();
    }
}
