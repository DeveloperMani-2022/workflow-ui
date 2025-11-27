namespace WorkflowEngine.Models.NodeExecutors;

/// <summary>
/// Interface that all node executors must implement
/// </summary>
public interface INodeExecutor
{
    /// <summary>
    /// The type of node this executor handles
    /// </summary>
    string NodeType { get; }
    
    /// <summary>
    /// Executes the node logic with the given context
    /// </summary>
    /// <param name="context">The execution context containing state and configuration</param>
    /// <returns>The result of the execution</returns>
    Task<NodeResult> ExecuteAsync(NodeContext context);
}
