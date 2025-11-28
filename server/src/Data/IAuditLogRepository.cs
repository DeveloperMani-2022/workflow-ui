using WorkflowEngine.Models;

namespace WorkflowEngine.Data;

/// <summary>
/// Interface for audit log repository operations
/// </summary>
public interface IAuditLogRepository
{
    Task<WorkflowAuditLog> CreateAsync(WorkflowAuditLog auditLog);
    Task<List<WorkflowAuditLog>> GetByWorkflowIdAsync(Guid workflowId);
}
