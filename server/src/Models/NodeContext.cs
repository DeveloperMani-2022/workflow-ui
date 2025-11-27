namespace WorkflowEngine.Models;

/// <summary>
/// Context object containing the state and data for workflow execution
/// </summary>
public class NodeContext
{
    /// <summary>
    /// Unique session identifier for this workflow execution
    /// </summary>
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Current node being executed
    /// </summary>
    public string CurrentNodeId { get; set; } = string.Empty;
    
    /// <summary>
    /// State variables that persist across node executions
    /// </summary>
    public Dictionary<string, object> StateVariables { get; set; } = new();
    
    /// <summary>
    /// History of executed nodes in this session
    /// </summary>
    public List<ExecutionHistoryEntry> ExecutionHistory { get; set; } = new();
    
    /// <summary>
    /// User input from the current or previous interaction
    /// </summary>
    public object? UserInput { get; set; }
    
    /// <summary>
    /// Additional metadata for the execution
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Configuration of the current node
    /// </summary>
    public Dictionary<string, object> NodeConfig { get; set; } = new();
    
    /// <summary>
    /// The workflow ID being executed
    /// </summary>
    public Guid WorkflowId { get; set; }
    
    /// <summary>
    /// User ID executing the workflow
    /// </summary>
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Represents a single entry in the execution history
/// </summary>
public class ExecutionHistoryEntry
{
    public string NodeId { get; set; } = string.Empty;
    public string NodeType { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? Output { get; set; }
}
