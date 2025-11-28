using Microsoft.Data.SqlClient;
using WorkflowEngine.Models;

namespace WorkflowEngine.Data;

/// <summary>
/// Repository for workflow version operations using ADO.NET
/// </summary>
public class WorkflowVersionRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    private readonly ILogger<WorkflowVersionRepository> _logger;
    
    public WorkflowVersionRepository(SqlConnectionFactory connectionFactory, ILogger<WorkflowVersionRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }
    
    /// <summary>
    /// Creates a new workflow version
    /// </summary>
    public async Task<WorkflowVersion> CreateAsync(WorkflowVersion version)
    {
        const string sql = @"
            INSERT INTO WorkflowVersions (Id, WorkflowId, VersionNumber, GraphJson, PublishedDate, PublishedBy, ReleaseNotes, IsActive)
            VALUES (@Id, @WorkflowId, @VersionNumber, @GraphJson, @PublishedDate, @PublishedBy, @ReleaseNotes, @IsActive)";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@Id", version.Id);
        command.Parameters.AddWithValue("@WorkflowId", version.WorkflowId);
        command.Parameters.AddWithValue("@VersionNumber", version.VersionNumber);
        command.Parameters.AddWithValue("@GraphJson", version.GraphJson);
        command.Parameters.AddWithValue("@PublishedDate", version.PublishedDate);
        command.Parameters.AddWithValue("@PublishedBy", version.PublishedBy);
        command.Parameters.AddWithValue("@ReleaseNotes", version.ReleaseNotes ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", version.IsActive);
        
        await command.ExecuteNonQueryAsync();
        
        _logger.LogInformation("Created workflow version {VersionId} for workflow {WorkflowId}", version.Id, version.WorkflowId);
        
        return version;
    }
    
    /// <summary>
    /// Gets the active version of a workflow
    /// </summary>
    public async Task<WorkflowVersion?> GetActiveVersionAsync(Guid workflowId)
    {
        const string sql = @"
            SELECT Id, WorkflowId, VersionNumber, GraphJson, PublishedDate, PublishedBy, ReleaseNotes, IsActive
            FROM WorkflowVersions
            WHERE WorkflowId = @WorkflowId AND IsActive = 1";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@WorkflowId", workflowId);
        
        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return MapWorkflowVersion(reader);
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all versions of a workflow
    /// </summary>
    public async Task<List<WorkflowVersion>> GetByWorkflowIdAsync(Guid workflowId)
    {
        const string sql = @"
            SELECT Id, WorkflowId, VersionNumber, GraphJson, PublishedDate, PublishedBy, ReleaseNotes, IsActive
            FROM WorkflowVersions
            WHERE WorkflowId = @WorkflowId
            ORDER BY PublishedDate DESC";
        
        var versions = new List<WorkflowVersion>();
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@WorkflowId", workflowId);
        
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            versions.Add(MapWorkflowVersion(reader));
        }
        
        return versions;
    }
    
    /// <summary>
    /// Checks if a version number already exists for a workflow
    /// </summary>
    public async Task<bool> VersionExistsAsync(Guid workflowId, string versionNumber)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM WorkflowVersions
            WHERE WorkflowId = @WorkflowId AND VersionNumber = @VersionNumber";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@WorkflowId", workflowId);
        command.Parameters.AddWithValue("@VersionNumber", versionNumber);
        
        var count = await command.ExecuteScalarAsync();
        return Convert.ToInt32(count) > 0;
    }
    
    /// <summary>
    /// Deactivates all versions of a workflow
    /// </summary>
    public async Task DeactivateAllVersionsAsync(Guid workflowId)
    {
        const string sql = @"
            UPDATE WorkflowVersions
            SET IsActive = 0
            WHERE WorkflowId = @WorkflowId";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@WorkflowId", workflowId);
        
        await command.ExecuteNonQueryAsync();
        
        _logger.LogInformation("Deactivated all versions for workflow {WorkflowId}", workflowId);
    }
    
    private WorkflowVersion MapWorkflowVersion(SqlDataReader reader)
    {
        return new WorkflowVersion
        {
            Id = reader.GetGuid(0),
            WorkflowId = reader.GetGuid(1),
            VersionNumber = reader.GetString(2),
            GraphJson = reader.GetString(3),
            PublishedDate = reader.GetDateTime(4),
            PublishedBy = reader.GetString(5),
            ReleaseNotes = reader.IsDBNull(6) ? null : reader.GetString(6),
            IsActive = reader.GetBoolean(7)
        };
    }
}
