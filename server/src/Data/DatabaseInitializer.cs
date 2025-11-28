using Microsoft.Data.SqlClient;

namespace WorkflowEngine.Data;

/// <summary>
/// Initializes the database schema on application startup
/// </summary>
public class DatabaseInitializer
{
    private readonly SqlConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;
    
    public DatabaseInitializer(SqlConnectionFactory connectionFactory, ILogger<DatabaseInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }
    
    /// <summary>
    /// Initializes the database by creating tables if they don't exist
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing database schema...");
            
            // Read the schema SQL file
            var schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "src", "Data", "schema.sql");
            
            // If running from bin directory, adjust path
            if (!File.Exists(schemaPath))
            {
                schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "Data", "schema.sql");
            }
            
            if (!File.Exists(schemaPath))
            {
                _logger.LogWarning("Schema file not found at {Path}. Skipping database initialization.", schemaPath);
                return;
            }
            
            var schemaSql = await File.ReadAllTextAsync(schemaPath);
            
            // Split by GO statements
            var batches = schemaSql.Split(new[] { "\nGO\n", "\nGO\r\n", "\r\nGO\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            using var connection = await _connectionFactory.CreateConnectionAsync();
            
            foreach (var batch in batches)
            {
                var trimmedBatch = batch.Trim();
                if (string.IsNullOrWhiteSpace(trimmedBatch))
                    continue;
                
                using var command = new SqlCommand(trimmedBatch, connection);
                command.CommandTimeout = 60;
                await command.ExecuteNonQueryAsync();
            }
            
            _logger.LogInformation("Database schema initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing database schema");
            throw;
        }
    }
}
