using WorkflowEngine.DTOs;
using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

/// <summary>
/// Service for validating workflow integrity and correctness
/// </summary>
public class WorkflowValidationService
{
    /// <summary>
    /// Validates a workflow and returns validation results
    /// </summary>
    public ValidationResult ValidateWorkflow(List<WorkflowNode> nodes, List<WorkflowEdge> edges)
    {
        var result = new ValidationResult { IsValid = true };
        
        // Check for start node
        var startNodes = nodes.Where(n => n.NodeType == "StartNode" || n.NodeType.Contains("Start")).ToList();
        if (startNodes.Count == 0)
        {
            result.Errors.Add(new ValidationError
            {
                Code = "NO_START_NODE",
                Message = "Workflow must have at least one start node"
            });
            result.IsValid = false;
        }
        else if (startNodes.Count > 1)
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "MULTIPLE_START_NODES",
                Message = "Workflow has multiple start nodes. Only the first one will be used."
            });
        }
        
        // Check for end node
        var endNodes = nodes.Where(n => n.NodeType == "EndNode" || n.NodeType.Contains("End")).ToList();
        if (endNodes.Count == 0)
        {
            result.Errors.Add(new ValidationError
            {
                Code = "NO_END_NODE",
                Message = "Workflow must have at least one end node"
            });
            result.IsValid = false;
        }
        
        // Check for orphan nodes (nodes with no connections)
        var connectedNodeIds = new HashSet<string>();
        foreach (var edge in edges)
        {
            connectedNodeIds.Add(edge.SourceNodeId);
            connectedNodeIds.Add(edge.TargetNodeId);
        }
        
        foreach (var node in nodes)
        {
            // Start and end nodes can be orphans in some cases
            if (node.NodeType != "StartNode" && node.NodeType != "EndNode" && !node.NodeType.Contains("Start") && !node.NodeType.Contains("End"))
            {
                if (!connectedNodeIds.Contains(node.NodeId))
                {
                    result.Warnings.Add(new ValidationWarning
                    {
                        Code = "ORPHAN_NODE",
                        Message = $"Node '{node.Label}' is not connected to any other nodes",
                        NodeId = node.NodeId
                    });
                }
            }
        }
        
        // Check for nodes with no outgoing connections (except end nodes)
        var nodesWithOutgoing = edges.Select(e => e.SourceNodeId).Distinct().ToHashSet();
        foreach (var node in nodes)
        {
            if (node.NodeType != "EndNode" && !node.NodeType.Contains("End"))
            {
                if (!nodesWithOutgoing.Contains(node.NodeId))
                {
                    result.Warnings.Add(new ValidationWarning
                    {
                        Code = "NO_OUTGOING_CONNECTION",
                        Message = $"Node '{node.Label}' has no outgoing connections",
                        NodeId = node.NodeId
                    });
                }
            }
        }
        
        // Check for circular dependencies (optional - can be allowed in some workflows)
        var circularPaths = DetectCircularDependencies(nodes, edges);
        if (circularPaths.Any())
        {
            result.Warnings.Add(new ValidationWarning
            {
                Code = "CIRCULAR_DEPENDENCY",
                Message = $"Workflow contains circular dependencies: {string.Join(", ", circularPaths)}"
            });
        }
        
        // Validate node configurations
        foreach (var node in nodes)
        {
            var nodeValidation = ValidateNodeConfiguration(node);
            if (!nodeValidation.IsValid)
            {
                result.Errors.AddRange(nodeValidation.Errors);
                result.IsValid = false;
            }
            result.Warnings.AddRange(nodeValidation.Warnings);
        }
        
        // Check for duplicate state keys
        var stateKeys = new Dictionary<string, List<string>>();
        foreach (var node in nodes)
        {
            if (node.NodeType == "QuestionNode" || node.NodeType == "StateUpdateNode")
            {
                // Parse config to check for state keys
                // This is a simplified check - in real implementation, parse the ConfigJson
                var configLower = node.ConfigJson.ToLower();
                if (configLower.Contains("statekey"))
                {
                    // Extract state key (simplified)
                    // In production, properly parse JSON
                }
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Validates individual node configuration
    /// </summary>
    private ValidationResult ValidateNodeConfiguration(WorkflowNode node)
    {
        var result = new ValidationResult { IsValid = true };
        
        switch (node.NodeType)
        {
            case "APICallNode":
                // Check if API URL is present
                if (!node.ConfigJson.Contains("apiUrl") || node.ConfigJson.Contains("\"apiUrl\":\"\""))
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "MISSING_API_URL",
                        Message = $"API Call node '{node.Label}' is missing API URL",
                        NodeId = node.NodeId
                    });
                    result.IsValid = false;
                }
                break;
                
            case "QuestionNode":
                // Check if prompt text is present
                if (!node.ConfigJson.Contains("promptText") || node.ConfigJson.Contains("\"promptText\":\"\""))
                {
                    result.Warnings.Add(new ValidationWarning
                    {
                        Code = "MISSING_PROMPT",
                        Message = $"Question node '{node.Label}' is missing prompt text",
                        NodeId = node.NodeId
                    });
                }
                break;
                
            case "ConditionNode":
                // Check if conditions are defined
                if (!node.ConfigJson.Contains("branches") || node.ConfigJson.Contains("\"branches\":[]"))
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "MISSING_CONDITIONS",
                        Message = $"Condition node '{node.Label}' has no condition branches defined",
                        NodeId = node.NodeId
                    });
                    result.IsValid = false;
                }
                break;
        }
        
        return result;
    }
    
    /// <summary>
    /// Detects circular dependencies in the workflow graph
    /// </summary>
    private List<string> DetectCircularDependencies(List<WorkflowNode> nodes, List<WorkflowEdge> edges)
    {
        var circularPaths = new List<string>();
        var graph = new Dictionary<string, List<string>>();
        
        // Build adjacency list
        foreach (var node in nodes)
        {
            graph[node.NodeId] = new List<string>();
        }
        
        foreach (var edge in edges)
        {
            if (graph.ContainsKey(edge.SourceNodeId))
            {
                graph[edge.SourceNodeId].Add(edge.TargetNodeId);
            }
        }
        
        // DFS to detect cycles
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();
        
        foreach (var nodeId in graph.Keys)
        {
            if (HasCycle(nodeId, graph, visited, recursionStack, new List<string>(), circularPaths))
            {
                // Cycle detected
            }
        }
        
        return circularPaths;
    }
    
    private bool HasCycle(string nodeId, Dictionary<string, List<string>> graph, 
        HashSet<string> visited, HashSet<string> recursionStack, 
        List<string> path, List<string> circularPaths)
    {
        if (recursionStack.Contains(nodeId))
        {
            // Found a cycle
            var cycleStart = path.IndexOf(nodeId);
            var cyclePath = string.Join(" -> ", path.Skip(cycleStart).Concat(new[] { nodeId }));
            if (!circularPaths.Contains(cyclePath))
            {
                circularPaths.Add(cyclePath);
            }
            return true;
        }
        
        if (visited.Contains(nodeId))
        {
            return false;
        }
        
        visited.Add(nodeId);
        recursionStack.Add(nodeId);
        path.Add(nodeId);
        
        if (graph.ContainsKey(nodeId))
        {
            foreach (var neighbor in graph[nodeId])
            {
                HasCycle(neighbor, graph, visited, recursionStack, path, circularPaths);
            }
        }
        
        recursionStack.Remove(nodeId);
        path.Remove(nodeId);
        
        return false;
    }
}
