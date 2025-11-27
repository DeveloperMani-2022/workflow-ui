namespace WorkflowEngine.Models;

/// <summary>
/// Represents an individual node within a workflow
/// </summary>
public class WorkflowNode
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    
    /// <summary>
    /// The node ID from the RappidJS graph
    /// </summary>
    public string NodeId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of node (Start, Message, Question, APICall, Condition, etc.)
    /// </summary>
    public string NodeType { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON configuration specific to this node type
    /// </summary>
    public string ConfigJson { get; set; } = string.Empty;
    
    /// <summary>
    /// X coordinate on the canvas
    /// </summary>
    public double PositionX { get; set; }
    
    /// <summary>
    /// Y coordinate on the canvas
    /// </summary>
    public double PositionY { get; set; }
    
    /// <summary>
    /// Display label for the node
    /// </summary>
    public string Label { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property to parent workflow
    /// </summary>
    public Workflow Workflow { get; set; } = null!;
}
