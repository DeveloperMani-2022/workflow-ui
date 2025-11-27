namespace WorkflowEngine.DTOs;

/// <summary>
/// DTO for workflow execution requests
/// </summary>
public class ExecuteWorkflowRequest
{
    public Guid WorkflowId { get; set; }
    public string? SessionId { get; set; }
    public Dictionary<string, object>? InitialState { get; set; }
    public object? UserInput { get; set; }
    public string UserId { get; set; } = "anonymous";
}

/// <summary>
/// DTO for workflow execution response
/// </summary>
public class ExecuteWorkflowResponse
{
    public string SessionId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Message { get; set; }
    public bool RequiresUserInput { get; set; }
    public bool IsComplete { get; set; }
    public string? CurrentNodeId { get; set; }
    public Dictionary<string, object> State { get; set; } = new();
    public Dictionary<string, object> Output { get; set; } = new();
    public List<ExecutionStep> ExecutionHistory { get; set; } = new();
}

/// <summary>
/// Represents a single execution step
/// </summary>
public class ExecutionStep
{
    public string NodeId { get; set; } = string.Empty;
    public string NodeType { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// DTO for publishing a workflow version
/// </summary>
public class PublishWorkflowRequest
{
    public string VersionNumber { get; set; } = string.Empty;
    public string? ReleaseNotes { get; set; }
}

/// <summary>
/// DTO for workflow validation response
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public List<ValidationWarning> Warnings { get; set; } = new();
}

public class ValidationError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? NodeId { get; set; }
}

public class ValidationWarning
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? NodeId { get; set; }
}
