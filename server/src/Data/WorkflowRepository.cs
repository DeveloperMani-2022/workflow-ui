using Microsoft.Data.SqlClient;
using System.Text.Json;
using WorkflowEngine.Models;

namespace WorkflowEngine.Data;

/// <summary>
/// Repository for workflow CRUD operations using ADO.NET
/// </summary>
public class WorkflowRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    private readonly ILogger<WorkflowRepository> _logger;
    
    public WorkflowRepository(SqlConnectionFactory connectionFactory, ILogger<WorkflowRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }
    
    /// <summary>
    /// Creates a new workflow
    /// </summary>
    public async Task<Workflow> CreateAsync(Workflow workflow)
    {
        const string sql = @"
            INSERT INTO Workflows (Id, Name, Description, GraphJson, CreatedDate, ModifiedDate, CreatedBy, ModifiedBy, IsPublished)
            VALUES (@Id, @Name, @Description, @GraphJson, @CreatedDate, @ModifiedDate, @CreatedBy, @ModifiedBy, @IsPublished)";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@Id", workflow.Id);
        command.Parameters.AddWithValue("@Name", workflow.Name);
        command.Parameters.AddWithValue("@Description", workflow.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@GraphJson", workflow.GraphJson);
        command.Parameters.AddWithValue("@CreatedDate", workflow.CreatedDate);
        command.Parameters.AddWithValue("@ModifiedDate", workflow.ModifiedDate);
        command.Parameters.AddWithValue("@CreatedBy", workflow.CreatedBy);
        command.Parameters.AddWithValue("@ModifiedBy", workflow.ModifiedBy);
        command.Parameters.AddWithValue("@IsPublished", workflow.IsPublished);
        
        await command.ExecuteNonQueryAsync();
        
        _logger.LogInformation("Created workflow {WorkflowId}", workflow.Id);
        
        return workflow;
    }
    
    /// <summary>
    /// Gets a workflow by ID
    /// </summary>
    public async Task<Workflow?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT Id, Name, Description, GraphJson, CreatedDate, ModifiedDate, CreatedBy, ModifiedBy, IsPublished
            FROM Workflows
            WHERE Id = @Id";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return MapWorkflow(reader);
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all workflows
    /// </summary>
    public async Task<List<Workflow>> GetAllAsync()
    {
        const string sql = @"
            SELECT Id, Name, Description, GraphJson, CreatedDate, ModifiedDate, CreatedBy, ModifiedBy, IsPublished
            FROM Workflows
            ORDER BY ModifiedDate DESC";
        
        var workflows = new List<Workflow>();
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            workflows.Add(MapWorkflow(reader));
        }
        
        return workflows;
    }
    
    /// <summary>
    /// Updates a workflow
    /// </summary>
    public async Task<bool> UpdateAsync(Workflow workflow)
    {
        const string sql = @"
            UPDATE Workflows
            SET Name = @Name,
                Description = @Description,
                GraphJson = @GraphJson,
                ModifiedDate = @ModifiedDate,
                ModifiedBy = @ModifiedBy,
                IsPublished = @IsPublished
            WHERE Id = @Id";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@Id", workflow.Id);
        command.Parameters.AddWithValue("@Name", workflow.Name);
        command.Parameters.AddWithValue("@Description", workflow.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@GraphJson", workflow.GraphJson);
        command.Parameters.AddWithValue("@ModifiedDate", workflow.ModifiedDate);
        command.Parameters.AddWithValue("@ModifiedBy", workflow.ModifiedBy);
        command.Parameters.AddWithValue("@IsPublished", workflow.IsPublished);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        
        _logger.LogInformation("Updated workflow {WorkflowId}", workflow.Id);
        
        return rowsAffected > 0;
    }
    
    /// <summary>
    /// Deletes a workflow
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        const string sql = "DELETE FROM Workflows WHERE Id = @Id";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        
        _logger.LogInformation("Deleted workflow {WorkflowId}", id);
        
        return rowsAffected > 0;
    }
    
    /// <summary>
    /// Gets the count of versions for a workflow
    /// </summary>
    public async Task<int> GetVersionCountAsync(Guid workflowId)
    {
        const string sql = "SELECT COUNT(*) FROM WorkflowVersions WHERE WorkflowId = @WorkflowId";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@WorkflowId", workflowId);
        
        var count = await command.ExecuteScalarAsync();
        return Convert.ToInt32(count);
    }
    
    private Workflow MapWorkflow(SqlDataReader reader)
    {
        return new Workflow
        {
            Id = reader.GetGuid(0),
            Name = reader.GetString(1),
            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            GraphJson = reader.GetString(3),
            CreatedDate = reader.GetDateTime(4),
            ModifiedDate = reader.GetDateTime(5),
            CreatedBy = reader.GetString(6),
            ModifiedBy = reader.GetString(7),
            IsPublished = reader.GetBoolean(8)
        };
    }
}
