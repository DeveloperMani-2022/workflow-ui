using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Data;
using WorkflowEngine.DTOs;
using WorkflowEngine.Models;
using WorkflowEngine.Services;

namespace WorkflowEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IWorkflowVersionRepository _versionRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IWorkflowPublisherService _publisherService;
    private readonly ILogger<WorkflowController> _logger;
    
    public WorkflowController(
        IWorkflowRepository workflowRepository,
        IWorkflowVersionRepository versionRepository,
        IAuditLogRepository auditLogRepository,
        IWorkflowPublisherService publisherService,
        ILogger<WorkflowController> logger)
    {
        _workflowRepository = workflowRepository;
        _versionRepository = versionRepository;
        _auditLogRepository = auditLogRepository;
        _publisherService = publisherService;
        _logger = logger;
    }
    
    /// <summary>
    /// Creates a new workflow
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkflowDTO>> CreateWorkflow([FromBody] CreateWorkflowRequest request)
    {
        try
        {
            var workflow = new Workflow
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                GraphJson = request.GraphJson ?? string.Empty,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                CreatedBy = "system", // TODO: Get from authentication
                ModifiedBy = "system",
                IsPublished = false
            };
            
            await _workflowRepository.CreateAsync(workflow);
            
            // Add audit log
            var auditLog = new WorkflowAuditLog
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflow.Id,
                Action = "Created",
                Timestamp = DateTime.UtcNow,
                UserId = "system"
            };
            await _auditLogRepository.CreateAsync(auditLog);
            
            _logger.LogInformation("Created workflow {WorkflowId} - {WorkflowName}", workflow.Id, workflow.Name);
            
            return CreatedAtAction(nameof(GetWorkflow), new { id = workflow.Id }, MapToDTO(workflow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow");
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Gets a workflow by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkflowDTO>> GetWorkflow(Guid id)
    {
        var workflow = await _workflowRepository.GetByIdAsync(id);
        
        if (workflow == null)
        {
            return NotFound();
        }
        
        return MapToDTO(workflow);
    }
    
    /// <summary>
    /// Gets all workflows
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult<List<WorkflowListItemDTO>>> GetWorkflows()
    {
        var workflows = await _workflowRepository.GetAllAsync();
        
        var workflowDTOs = new List<WorkflowListItemDTO>();
        
        foreach (var workflow in workflows)
        {
            var versionCount = await _workflowRepository.GetVersionCountAsync(workflow.Id);
            
            workflowDTOs.Add(new WorkflowListItemDTO
            {
                Id = workflow.Id,
                Name = workflow.Name,
                Description = workflow.Description,
                CreatedDate = workflow.CreatedDate,
                ModifiedDate = workflow.ModifiedDate,
                IsPublished = workflow.IsPublished,
                VersionCount = versionCount
            });
        }
        
        return workflowDTOs;
    }
    
    /// <summary>
    /// Updates a workflow
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<WorkflowDTO>> UpdateWorkflow(Guid id, [FromBody] UpdateWorkflowRequest request)
    {
        try
        {
            var workflow = await _workflowRepository.GetByIdAsync(id);
            
            if (workflow == null)
            {
                return NotFound();
            }
            
            // Update fields
            if (request.Name != null)
                workflow.Name = request.Name;
            
            if (request.Description != null)
                workflow.Description = request.Description;
            
            if (request.GraphJson != null)
            {
                workflow.GraphJson = request.GraphJson;
            }
            
            workflow.ModifiedDate = DateTime.UtcNow;
            workflow.ModifiedBy = "system"; // TODO: Get from authentication
            
            await _workflowRepository.UpdateAsync(workflow);
            
            // Add audit log
            var auditLog = new WorkflowAuditLog
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflow.Id,
                Action = "Updated",
                Timestamp = DateTime.UtcNow,
                UserId = "system"
            };
            await _auditLogRepository.CreateAsync(auditLog);
            
            _logger.LogInformation("Updated workflow {WorkflowId}", workflow.Id);
            
            return MapToDTO(workflow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workflow {WorkflowId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Deletes a workflow
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkflow(Guid id)
    {
        var deleted = await _workflowRepository.DeleteAsync(id);
        
        if (!deleted)
        {
            return NotFound();
        }
        
        _logger.LogInformation("Deleted workflow {WorkflowId}", id);
        
        return NoContent();
    }
    
    /// <summary>
    /// Validates a workflow
    /// </summary>
    [HttpPost("{id}/validate")]
    public async Task<ActionResult<ValidationResult>> ValidateWorkflow(Guid id)
    {
        var workflow = await _workflowRepository.GetByIdAsync(id);
        
        if (workflow == null)
        {
            return NotFound();
        }
        
        // For validation, we would need to parse the GraphJson to extract nodes and edges
        // Since we removed the compiler service, we'll return a simplified validation
        // In a real implementation, you would parse the GraphJson here
        
        var validationResult = new ValidationResult 
        { 
            IsValid = !string.IsNullOrEmpty(workflow.GraphJson),
            Errors = new List<ValidationError>(),
            Warnings = new List<ValidationWarning>()
        };
        
        if (string.IsNullOrEmpty(workflow.GraphJson))
        {
            validationResult.Errors.Add(new ValidationError
            {
                Code = "EMPTY_GRAPH",
                Message = "Workflow graph is empty"
            });
        }
        
        return validationResult;
    }
    
    /// <summary>
    /// Publishes a workflow version
    /// </summary>
    [HttpPost("{id}/publish")]
    public async Task<ActionResult<WorkflowVersion>> PublishWorkflow(Guid id, [FromBody] PublishWorkflowRequest request)
    {
        try
        {
            var version = await _publisherService.PublishVersionAsync(
                id, 
                request.VersionNumber, 
                "system", // TODO: Get from authentication
                request.ReleaseNotes);
            
            _logger.LogInformation("Published workflow {WorkflowId} version {Version}", id, request.VersionNumber);
            
            return version;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing workflow {WorkflowId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }
    
    private WorkflowDTO MapToDTO(Workflow workflow)
    {
        return new WorkflowDTO
        {
            Id = workflow.Id,
            Name = workflow.Name,
            Description = workflow.Description,
            GraphJson = workflow.GraphJson,
            CreatedDate = workflow.CreatedDate,
            ModifiedDate = workflow.ModifiedDate,
            IsPublished = workflow.IsPublished
        };
    }
}
