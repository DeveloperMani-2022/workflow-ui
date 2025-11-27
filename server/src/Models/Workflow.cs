namespace WorkflowEngine.Models;

/// <summary>
/// Represents a workflow with its metadata and graph definition
/// </summary>
public class Workflow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON representation of the RappidJS graph
    /// </summary>
    public string GraphJson { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates if this workflow is currently published
    /// </summary>
    public bool IsPublished { get; set; }
    
    /// <summary>
    /// Navigation property for workflow versions
    /// </summary>
    public ICollection<WorkflowVersion> Versions { get; set; } = new List<WorkflowVersion>();
    
    /// <summary>
    /// Navigation property for workflow nodes
    /// </summary>
    public ICollection<WorkflowNode> Nodes { get; set; } = new List<WorkflowNode>();
    
    /// <summary>
    /// Navigation property for workflow edges
    /// </summary>
    public ICollection<WorkflowEdge> Edges { get; set; } = new List<WorkflowEdge>();
    
    /// <summary>
    /// Navigation property for audit logs
    /// </summary>
    public ICollection<WorkflowAuditLog> AuditLogs { get; set; } = new List<WorkflowAuditLog>();
}
