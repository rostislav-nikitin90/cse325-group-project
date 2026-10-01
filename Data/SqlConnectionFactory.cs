using Microsoft.Data.SqlClient;

namespace cse325_group_project.Data;

// Factory interface for creating database connections to Azure SQL Database.
public interface IDbConnectionFactory
{
    // Creates and returns a new SqlConnection using the configured connection string.
    SqlConnection CreateConnection();
}

// Implementation of IDbConnectionFactory that retrieves the 'DefaultConnection'
// connection string from application configuration / Secret Manager and instantiates SQL connections.
public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    // Initializes a new instance of the SqlConnectionFactory class.
    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration or user-secrets.");
    }

    // Creates and returns an unopened SqlConnection configured for Azure SQL Database.
    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
