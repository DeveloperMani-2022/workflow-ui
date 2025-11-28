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

/// <summary>
/// DTO for validation results
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public List<ValidationWarning> Warnings { get; set; } = new();
}

/// <summary>
/// DTO for validation errors
/// </summary>
public class ValidationError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? NodeId { get; set; }
}

/// <summary>
/// DTO for validation warnings
/// </summary>
public class ValidationWarning
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? NodeId { get; set; }
}

/// <summary>
/// DTO for publishing a workflow version
/// </summary>
public class PublishWorkflowRequest
{
    public string VersionNumber { get; set; } = string.Empty;
    public string? ReleaseNotes { get; set; }
}
