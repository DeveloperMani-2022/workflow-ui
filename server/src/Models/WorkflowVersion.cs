namespace WorkflowEngine.Models;

/// <summary>
/// Represents a published version of a workflow
/// </summary>
public class WorkflowVersion
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    
    /// <summary>
    /// Semantic version number (e.g., "1.0.0")
    /// </summary>
    public string VersionNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Snapshot of the workflow graph at the time of publishing
    /// </summary>
    public string GraphJson { get; set; } = string.Empty;
    
    /// <summary>
    /// When this version was published
    /// </summary>
    public DateTime PublishedDate { get; set; }
    
    /// <summary>
    /// Who published this version
    /// </summary>
    public string PublishedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional release notes for this version
    /// </summary>
    public string? ReleaseNotes { get; set; }
    
    /// <summary>
    /// Indicates if this is the currently active version
    /// </summary>
    public bool IsActive { get; set; }
}
