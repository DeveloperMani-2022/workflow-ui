using System.Text.Json;
using WorkflowEngine.Data;
using WorkflowEngine.DTOs;
using WorkflowEngine.Models;
using WorkflowEngine.Models.NodeExecutors;
using Microsoft.EntityFrameworkCore;

namespace WorkflowEngine.Services;

/// <summary>
/// Service for orchestrating workflow execution
/// </summary>
public class WorkflowExecutionService
{
    private readonly ApplicationDbContext _context;
    private readonly IEnumerable<INodeExecutor> _nodeExecutors;
    private readonly WorkflowCompilerService _compilerService;
    
    // In-memory session storage (in production, use Redis or similar)
    private static readonly Dictionary<string, NodeContext> _sessions = new();
    
    public WorkflowExecutionService(
        ApplicationDbContext context,
        IEnumerable<INodeExecutor> nodeExecutors,
        WorkflowCompilerService compilerService)
    {
        _context = context;
        _nodeExecutors = nodeExecutors;
        _compilerService = compilerService;
    }
    
    /// <summary>
    /// Executes a workflow or continues execution from a paused state
    /// </summary>
    public async Task<ExecuteWorkflowResponse> ExecuteWorkflowAsync(ExecuteWorkflowRequest request)
    {
        try
        {
            NodeContext context;
            
            // Check if this is a continuation of an existing session
            if (!string.IsNullOrEmpty(request.SessionId) && _sessions.TryGetValue(request.SessionId, out var existingContext))
            {
                context = existingContext;
                context.UserInput = request.UserInput;
            }
            else
            {
                // New execution - load workflow and initialize context
                var workflow = await _context.Workflows
                    .Include(w => w.Nodes)
                    .Include(w => w.Edges)
                    .FirstOrDefaultAsync(w => w.Id == request.WorkflowId);
                
                if (workflow == null)
                {
                    return new ExecuteWorkflowResponse
                    {
                        Success = false,
                        ErrorMessage = "Workflow not found"
                    };
                }
                
                // Find start node
                var startNode = workflow.Nodes.FirstOrDefault(n => n.NodeType == "StartNode" || n.NodeType.Contains("Start"));
                if (startNode == null)
                {
                    return new ExecuteWorkflowResponse
                    {
                        Success = false,
                        ErrorMessage = "Workflow has no start node"
                    };
                }
                
                // Initialize context
                context = new NodeContext
                {
                    SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                    WorkflowId = request.WorkflowId,
                    CurrentNodeId = startNode.NodeId,
                    UserId = request.UserId,
                    StateVariables = request.InitialState ?? new Dictionary<string, object>(),
                    UserInput = request.UserInput
                };
                
                // Store session
                _sessions[context.SessionId] = context;
            }
            
            // Execute workflow steps
            var response = await ExecuteStepsAsync(context);
            
            // Log execution
            await LogExecutionAsync(context, response.Success);
            
            return response;
        }
        catch (Exception ex)
        {
            return new ExecuteWorkflowResponse
            {
                Success = false,
                ErrorMessage = $"Workflow execution failed: {ex.Message}"
            };
        }
    }
    
    /// <summary>
    /// Executes workflow steps until completion or user input is required
    /// </summary>
    private async Task<ExecuteWorkflowResponse> ExecuteStepsAsync(NodeContext context)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Nodes)
            .Include(w => w.Edges)
            .FirstOrDefaultAsync(w => w.Id == context.WorkflowId);
        
        if (workflow == null)
        {
            return new ExecuteWorkflowResponse
            {
                Success = false,
                ErrorMessage = "Workflow not found"
            };
        }
        
        var maxIterations = 100; // Prevent infinite loops
        var iterations = 0;
        
        while (!string.IsNullOrEmpty(context.CurrentNodeId) && iterations < maxIterations)
        {
            iterations++;
            
            // Get current node
            var currentNode = workflow.Nodes.FirstOrDefault(n => n.NodeId == context.CurrentNodeId);
            if (currentNode == null)
            {
                return new ExecuteWorkflowResponse
                {
                    SessionId = context.SessionId,
                    Success = false,
                    ErrorMessage = $"Node not found: {context.CurrentNodeId}",
                    State = context.StateVariables
                };
            }
            
            // Check if this is an end node
            if (currentNode.NodeType == "EndNode" || currentNode.NodeType.Contains("End"))
            {
                return new ExecuteWorkflowResponse
                {
                    SessionId = context.SessionId,
                    Success = true,
                    IsComplete = true,
                    Message = "Workflow completed successfully",
                    State = context.StateVariables,
                    ExecutionHistory = MapExecutionHistory(context.ExecutionHistory)
                };
            }
            
            // Execute current node
            var executor = _nodeExecutors.FirstOrDefault(e => e.NodeType == currentNode.NodeType);
            if (executor == null)
            {
                return new ExecuteWorkflowResponse
                {
                    SessionId = context.SessionId,
                    Success = false,
                    ErrorMessage = $"No executor found for node type: {currentNode.NodeType}",
                    State = context.StateVariables
                };
            }
            
            // Parse node configuration
            context.NodeConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(currentNode.ConfigJson) 
                ?? new Dictionary<string, object>();
            
            // Execute node
            var result = await executor.ExecuteAsync(context);
            
            // Add to execution history
            context.ExecutionHistory.Add(new ExecutionHistoryEntry
            {
                NodeId = currentNode.NodeId,
                NodeType = currentNode.NodeType,
                ExecutedAt = DateTime.UtcNow,
                Success = result.Success,
                ErrorMessage = result.ErrorMessage,
                Output = result.Output
            });
            
            // Check if execution failed
            if (!result.Success)
            {
                return new ExecuteWorkflowResponse
                {
                    SessionId = context.SessionId,
                    Success = false,
                    ErrorMessage = result.ErrorMessage,
                    CurrentNodeId = context.CurrentNodeId,
                    State = context.StateVariables,
                    ExecutionHistory = MapExecutionHistory(context.ExecutionHistory)
                };
            }
            
            // Apply state updates
            if (result.StateUpdates != null)
            {
                foreach (var update in result.StateUpdates)
                {
                    context.StateVariables[update.Key] = update.Value;
                }
            }
            
            // Check if user input is required
            if (result.RequiresUserInput)
            {
                return new ExecuteWorkflowResponse
                {
                    SessionId = context.SessionId,
                    Success = true,
                    RequiresUserInput = true,
                    Message = result.Message,
                    CurrentNodeId = context.CurrentNodeId,
                    State = context.StateVariables,
                    Output = result.Output,
                    ExecutionHistory = MapExecutionHistory(context.ExecutionHistory)
                };
            }
            
            // Determine next node
            if (!string.IsNullOrEmpty(result.NextNodeId))
            {
                // Explicit next node specified
                context.CurrentNodeId = result.NextNodeId;
            }
            else
            {
                // Find next node from edges
                var nextEdge = workflow.Edges.FirstOrDefault(e => 
                    e.SourceNodeId == context.CurrentNodeId && 
                    (string.IsNullOrEmpty(result.NextPort) || e.SourcePort == result.NextPort));
                
                if (nextEdge != null)
                {
                    context.CurrentNodeId = nextEdge.TargetNodeId;
                }
                else
                {
                    // No next node found - workflow may be incomplete
                    return new ExecuteWorkflowResponse
                    {
                        SessionId = context.SessionId,
                        Success = true,
                        IsComplete = true,
                        Message = "Workflow execution ended (no next node found)",
                        State = context.StateVariables,
                        ExecutionHistory = MapExecutionHistory(context.ExecutionHistory)
                    };
                }
            }
            
            // Clear user input for next iteration
            context.UserInput = null;
        }
        
        if (iterations >= maxIterations)
        {
            return new ExecuteWorkflowResponse
            {
                SessionId = context.SessionId,
                Success = false,
                ErrorMessage = "Workflow execution exceeded maximum iterations (possible infinite loop)",
                State = context.StateVariables
            };
        }
        
        return new ExecuteWorkflowResponse
        {
            SessionId = context.SessionId,
            Success = true,
            State = context.StateVariables,
            ExecutionHistory = MapExecutionHistory(context.ExecutionHistory)
        };
    }
    
    /// <summary>
    /// Logs workflow execution to audit log
    /// </summary>
    private async Task LogExecutionAsync(NodeContext context, bool success)
    {
        var auditLog = new WorkflowAuditLog
        {
            Id = Guid.NewGuid(),
            WorkflowId = context.WorkflowId,
            Action = success ? "Executed" : "ExecutionFailed",
            Timestamp = DateTime.UtcNow,
            UserId = context.UserId,
            SessionId = context.SessionId,
            Details = JsonSerializer.Serialize(new
            {
                NodesExecuted = context.ExecutionHistory.Count,
                FinalState = context.StateVariables
            })
        };
        
        _context.WorkflowAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
    
    private List<ExecutionStep> MapExecutionHistory(List<ExecutionHistoryEntry> history)
    {
        return history.Select(h => new ExecutionStep
        {
            NodeId = h.NodeId,
            NodeType = h.NodeType,
            ExecutedAt = h.ExecutedAt,
            Success = h.Success,
            ErrorMessage = h.ErrorMessage
        }).ToList();
    }
}
