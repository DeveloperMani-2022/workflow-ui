using Microsoft.Data.SqlClient;

namespace WorkflowEngine.Data;

/// <summary>
/// Factory for creating SQL Server connections
/// </summary>
public class SqlConnectionFactory
{
    private readonly string _connectionString;
    
    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration");
    }
    
    /// <summary>
    /// Creates and opens a new SQL connection
    /// </summary>
    public async Task<SqlConnection> CreateConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
    
    /// <summary>
    /// Creates a new SQL connection (not opened)
    /// </summary>
    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
