using System.Data;
using cse325_group_project.Data;
using cse325_group_project.Models;
using Microsoft.Data.SqlClient;

namespace cse325_group_project.Services;

// Service contract for handling HR user authentication, creation, and database retrieval.
public interface IAuthService
{
    // Validates user credentials against the database and cryptographic password hash.
    Task<HrUser?> AuthenticateAsync(string email, string password);

    // Creates and inserts a new HR user record into the database with a hashed password.
    Task<bool> CreateHrUserAsync(int hrId, string firstName, string lastName, string email, string plaintextPassword);

    // Retrieves an HR user record from Azure SQL Database by their email address.
    Task<HrUser?> GetHrByEmailAsync(string email);
}

// Service responsible for authenticating and creating HR users against the Azure SQL database.
// Executes parameterized queries to prevent SQL injection and hashes passwords via BCrypt.
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

    // Hashes the plaintext password and inserts the new HR user record into [dbo].[hr].
    public async Task<bool> CreateHrUserAsync(int hrId, string firstName, string lastName, string email, string plaintextPassword)
    {
        // Hash password using BCrypt before storing
        string passwordHash = _passwordHasher.HashPassword(plaintextPassword);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string query = @"
            INSERT INTO dbo.hr (hr_id, first_name, last_name, email, password)
            VALUES (@HrId, @FirstName, @LastName, @Email, @Password)";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.Add("@HrId", SqlDbType.Int).Value = hrId;
        command.Parameters.Add("@FirstName", SqlDbType.VarChar, 50).Value = firstName.Trim();
        command.Parameters.Add("@LastName", SqlDbType.VarChar, 50).Value = lastName.Trim();
        command.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = email.Trim();
        command.Parameters.Add("@Password", SqlDbType.VarChar, 255).Value = passwordHash;

        int rowsAffected = await command.ExecuteNonQueryAsync();
        _logger.LogInformation("Successfully created HR user {Email} with ID {HrId}.", email, hrId);
        return rowsAffected > 0;
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
