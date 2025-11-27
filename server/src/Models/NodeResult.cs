namespace WorkflowEngine.Models;

/// <summary>
/// Result returned by node executors after execution
/// </summary>
public class NodeResult
{
    /// <summary>
    /// Indicates if the node executed successfully
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if execution failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Output data from the node execution
    /// </summary>
    public Dictionary<string, object> Output { get; set; } = new();
    
    /// <summary>
    /// The next node ID to execute (null for end nodes)
    /// </summary>
    public string? NextNodeId { get; set; }
    
    /// <summary>
    /// For branching nodes, the port/path to follow
    /// </summary>
    public string? NextPort { get; set; }
    
    /// <summary>
    /// Indicates if the workflow should pause for user input
    /// </summary>
    public bool RequiresUserInput { get; set; }
    
    /// <summary>
    /// Message to display to the user
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// State updates to apply after this node
    /// </summary>
    public Dictionary<string, object>? StateUpdates { get; set; }
    
    /// <summary>
    /// Indicates if this is the end of the workflow
    /// </summary>
    public bool IsComplete { get; set; }
    
    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static NodeResult SuccessResult(string? nextNodeId = null, string? message = null)
    {
        return new NodeResult
        {
            Success = true,
            NextNodeId = nextNodeId,
            Message = message
        };
    }
    
    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static NodeResult FailureResult(string errorMessage)
    {
        return new NodeResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
    
    /// <summary>
    /// Creates a result that requires user input
    /// </summary>
    public static NodeResult UserInputRequired(string message)
    {
        return new NodeResult
        {
            Success = true,
            RequiresUserInput = true,
            Message = message
        };
    }
}
