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
}
