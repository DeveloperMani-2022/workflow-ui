namespace WorkflowEngine.Models;

/// <summary>
/// Represents a connection (edge) between two workflow nodes
/// </summary>
public class WorkflowEdge
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    
    /// <summary>
    /// The edge ID from the RappidJS graph
    /// </summary>
    public string EdgeId { get; set; } = string.Empty;
    
    /// <summary>
    /// Source node ID (from RappidJS)
    /// </summary>
    public string SourceNodeId { get; set; } = string.Empty;
    
    /// <summary>
    /// Target node ID (from RappidJS)
    /// </summary>
    public string TargetNodeId { get; set; } = string.Empty;
    
    /// <summary>
    /// Source port name (for nodes with multiple outputs)
    /// </summary>
    public string? SourcePort { get; set; }
    
    /// <summary>
    /// Target port name (for nodes with multiple inputs)
    /// </summary>
    public string? TargetPort { get; set; }
    
    /// <summary>
    /// Optional label for the edge (e.g., "Yes", "No" for conditions)
    /// </summary>
    public string? Label { get; set; }
    
    /// <summary>
    /// Navigation property to parent workflow
    /// </summary>
    public Workflow Workflow { get; set; } = null!;
}
