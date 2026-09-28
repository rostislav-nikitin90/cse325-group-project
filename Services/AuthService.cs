using System.Data;
using cse325_group_project.Data;
using cse325_group_project.Models;
using Microsoft.Data.SqlClient;

namespace cse325_group_project.Services;

// Service contract for handling HR user authentication and database user retrieval.
public interface IAuthService
{
    // Validates user credentials against the database and cryptographic password hash.
    Task<HrUser?> AuthenticateAsync(string email, string password);

    // Retrieves an HR user record from Azure SQL Database by their email address.
    Task<HrUser?> GetHrByEmailAsync(string email);
}

// Service responsible for authenticating HR users against the Azure SQL database.
// Executes parameterized queries to prevent SQL injection and validates passwords via BCrypt.
public class AuthService : IAuthService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    // Initializes a new instance of the AuthService class.
    public AuthService(
        IDbConnectionFactory connectionFactory,
        IPasswordHasher passwordHasher,
        ILogger<AuthService> logger)
    {
        _connectionFactory = connectionFactory;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    // Authenticates an HR user by querying the user record by email and comparing
    // the provided password with the stored BCrypt hash.
    public async Task<HrUser?> AuthenticateAsync(string email, string password)
    {
        var user = await GetHrByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("Authentication failed: User with email {Email} not found.", email);
            return null;
        }

        bool isValid = _passwordHasher.VerifyPassword(password, user.PasswordHash);
        if (!isValid)
        {
            _logger.LogWarning("Authentication failed: Invalid credentials for {Email}.", email);
            return null;
        }

        _logger.LogInformation("Authentication succeeded for {Email}.", email);
        return user;
    }

    // Executes a parameterized query to fetch an HR user record from [dbo].[hr] by email.
    public async Task<HrUser?> GetHrByEmailAsync(string email)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string query = @"
            SELECT hr_id, first_name, last_name, email, password
            FROM dbo.hr
            WHERE email = @Email";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = email.Trim();

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new HrUser
            {
                HrId = reader.GetInt32(reader.GetOrdinal("hr_id")),
                FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                LastName = reader.GetString(reader.GetOrdinal("last_name")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("password"))
            };
        }

        return null;
    }
}
