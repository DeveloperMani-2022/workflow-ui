using System.Text.Json;
using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

/// <summary>
/// Service for compiling Agent Graph JSON into workflow models
/// </summary>
public class WorkflowCompilerService
{
    private readonly JsonSerializerOptions _jsonOptions;

    public WorkflowCompilerService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Compiles an Agent Graph JSON into workflow nodes and edges
    /// </summary>
    public (List<WorkflowNode> Nodes, List<WorkflowEdge> Edges) CompileGraph(string graphJson, Guid workflowId)
    {
        var nodes = new List<WorkflowNode>();
        var edges = new List<WorkflowEdge>();
        
        try
        {
            // Deserialize as a dictionary of nodes (AgentGraphDefinition)
            var graphData = JsonSerializer.Deserialize<AgentGraphDefinition>(graphJson, _jsonOptions);
            if (graphData == null)
            {
                throw new InvalidOperationException("Failed to parse graph JSON");
            }
            
            foreach (var kvp in graphData)
            {
                var nodeId = kvp.Key;
                var agentNode = kvp.Value;
                
                // Create WorkflowNode
                var node = new WorkflowNode
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = workflowId,
                    NodeId = nodeId,
                    NodeType = agentNode.Type,
                    Label = agentNode.Label,
                    // Position is not in the flat JSON for all nodes (except 'note'), so we default to 0
                    PositionX = 0, 
                    PositionY = 0,
                    ConfigJson = JsonSerializer.Serialize(agentNode, _jsonOptions)
                };
                
                // Handle specific node types if needed for specific properties
                if (agentNode is NoteNode noteNode)
                {
                    // Note nodes might have custom position data in the JSON, but our base AgentGraphNode doesn't map it generic enough yet.
                    // For now, we store the full object in ConfigJson.
                }

                nodes.Add(node);

                // Create Edges from 'To' list
                if (agentNode.To != null)
                {
                    foreach (var connection in agentNode.To)
                    {
                        var edge = new WorkflowEdge
                        {
                            Id = Guid.NewGuid(),
                            WorkflowId = workflowId,
                            EdgeId = Guid.NewGuid().ToString(), // Edges don't have explicit IDs in this format
                            SourceNodeId = nodeId,
                            TargetNodeId = connection.Id,
                            Label = connection.Prompt // Use prompt as label (e.g. "Solved", "High priority")
                        };
                        edges.Add(edge);
                    }
                }
            }
            
            return (nodes, edges);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to compile graph: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Builds an execution graph from nodes and edges
    /// </summary>
    public Dictionary<string, List<string>> BuildExecutionGraph(List<WorkflowNode> nodes, List<WorkflowEdge> edges)
    {
        var graph = new Dictionary<string, List<string>>();
        
        // Initialize graph with all nodes
        foreach (var node in nodes)
        {
            graph[node.NodeId] = new List<string>();
        }
        
        // Add edges
        foreach (var edge in edges)
        {
            if (graph.ContainsKey(edge.SourceNodeId))
            {
                graph[edge.SourceNodeId].Add(edge.TargetNodeId);
            }
        }
        
        return graph;
    }
    
    /// <summary>
    /// Finds the start node in the workflow
    /// </summary>
    public WorkflowNode? FindStartNode(List<WorkflowNode> nodes)
    {
        // In the demo JSON, the start node is explicitly keyed as "start"
        return nodes.FirstOrDefault(n => n.NodeId == "start");
    }
}
