namespace WorkflowEngine.Models;

/// <summary>
/// Audit log for tracking workflow changes and executions
/// </summary>
public class WorkflowAuditLog
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    
    /// <summary>
    /// Action performed (Created, Updated, Deleted, Executed, Published, etc.)
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// When the action occurred
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// User who performed the action
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional details about the action (JSON format)
    /// </summary>
    public string? Details { get; set; }
    
    /// <summary>
    /// Session ID for execution tracking
    /// </summary>
    public string? SessionId { get; set; }
}
