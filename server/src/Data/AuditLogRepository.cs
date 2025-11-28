using Microsoft.Data.SqlClient;
using WorkflowEngine.Models;

namespace WorkflowEngine.Data;

/// <summary>
/// Repository for audit log operations using ADO.NET
/// </summary>
public class AuditLogRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    private readonly ILogger<AuditLogRepository> _logger;
    
    public AuditLogRepository(SqlConnectionFactory connectionFactory, ILogger<AuditLogRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }
    
    /// <summary>
    /// Creates a new audit log entry
    /// </summary>
    public async Task<WorkflowAuditLog> CreateAsync(WorkflowAuditLog auditLog)
    {
        const string sql = @"
            INSERT INTO WorkflowAuditLogs (Id, WorkflowId, Action, Timestamp, UserId, Details)
            VALUES (@Id, @WorkflowId, @Action, @Timestamp, @UserId, @Details)";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@Id", auditLog.Id);
        command.Parameters.AddWithValue("@WorkflowId", auditLog.WorkflowId);
        command.Parameters.AddWithValue("@Action", auditLog.Action);
        command.Parameters.AddWithValue("@Timestamp", auditLog.Timestamp);
        command.Parameters.AddWithValue("@UserId", auditLog.UserId);
        command.Parameters.AddWithValue("@Details", auditLog.Details ?? (object)DBNull.Value);
        
        await command.ExecuteNonQueryAsync();
        
        _logger.LogDebug("Created audit log for workflow {WorkflowId}: {Action}", auditLog.WorkflowId, auditLog.Action);
        
        return auditLog;
    }
    
    /// <summary>
    /// Gets all audit logs for a workflow
    /// </summary>
    public async Task<List<WorkflowAuditLog>> GetByWorkflowIdAsync(Guid workflowId)
    {
        const string sql = @"
            SELECT Id, WorkflowId, Action, Timestamp, UserId, Details
            FROM WorkflowAuditLogs
            WHERE WorkflowId = @WorkflowId
            ORDER BY Timestamp DESC";
        
        var logs = new List<WorkflowAuditLog>();
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@WorkflowId", workflowId);
        
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            logs.Add(MapAuditLog(reader));
        }
        
        return logs;
    }
    
    private WorkflowAuditLog MapAuditLog(SqlDataReader reader)
    {
        return new WorkflowAuditLog
        {
            Id = reader.GetGuid(0),
            WorkflowId = reader.GetGuid(1),
            Action = reader.GetString(2),
            Timestamp = reader.GetDateTime(3),
            UserId = reader.GetString(4),
            Details = reader.IsDBNull(5) ? null : reader.GetString(5)
        };
    }
}
