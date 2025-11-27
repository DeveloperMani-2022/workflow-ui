using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.DTOs;
using WorkflowEngine.Services;

namespace WorkflowEngine.Controllers;

[ApiController]
[Route("api/workflow")]
public class WorkflowExecutionController : ControllerBase
{
    private readonly WorkflowExecutionService _executionService;
    private readonly ILogger<WorkflowExecutionController> _logger;
    
    public WorkflowExecutionController(
        WorkflowExecutionService executionService,
        ILogger<WorkflowExecutionController> logger)
    {
        _executionService = executionService;
        _logger = logger;
    }
    
    /// <summary>
    /// Executes a workflow or continues execution from a paused state
    /// </summary>
    [HttpPost("execute")]
    public async Task<ActionResult<ExecuteWorkflowResponse>> ExecuteWorkflow([FromBody] ExecuteWorkflowRequest request)
    {
        try
        {
            _logger.LogInformation("Executing workflow {WorkflowId}, Session: {SessionId}", 
                request.WorkflowId, request.SessionId ?? "new");
            
            var response = await _executionService.ExecuteWorkflowAsync(request);
            
            if (!response.Success)
            {
                _logger.LogWarning("Workflow execution failed: {Error}", response.ErrorMessage);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow {WorkflowId}", request.WorkflowId);
            return BadRequest(new ExecuteWorkflowResponse
            {
                Success = false,
                ErrorMessage = $"Execution error: {ex.Message}"
            });
        }
    }
}
