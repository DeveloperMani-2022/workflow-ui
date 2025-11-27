namespace WorkflowEngine.DTOs;

/// <summary>
/// Data transfer object for workflow operations
/// </summary>
public class WorkflowDTO
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GraphJson { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsPublished { get; set; }
}

/// <summary>
/// DTO for creating a new workflow
/// </summary>
public class CreateWorkflowRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GraphJson { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating an existing workflow
/// </summary>
public class UpdateWorkflowRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? GraphJson { get; set; }
}

/// <summary>
/// DTO for workflow list response
/// </summary>
public class WorkflowListItemDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsPublished { get; set; }
    public int VersionCount { get; set; }
}
