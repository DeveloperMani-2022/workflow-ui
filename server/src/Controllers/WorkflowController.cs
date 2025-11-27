using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Data;
using WorkflowEngine.DTOs;
using WorkflowEngine.Models;
using WorkflowEngine.Services;

namespace WorkflowEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly WorkflowCompilerService _compilerService;
    private readonly WorkflowValidationService _validationService;
    private readonly WorkflowPublisherService _publisherService;
    private readonly ILogger<WorkflowController> _logger;
    
    public WorkflowController(
        ApplicationDbContext context,
        WorkflowCompilerService compilerService,
        WorkflowValidationService validationService,
        WorkflowPublisherService publisherService,
        ILogger<WorkflowController> logger)
    {
        _context = context;
        _compilerService = compilerService;
        _validationService = validationService;
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
                GraphJson = request.GraphJson,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                CreatedBy = "system", // TODO: Get from authentication
                ModifiedBy = "system",
                IsPublished = false
            };
            
            // Compile graph to nodes and edges
            if (!string.IsNullOrEmpty(request.GraphJson))
            {
                var (nodes, edges) = _compilerService.CompileGraph(request.GraphJson, workflow.Id);
                workflow.Nodes = nodes;
                workflow.Edges = edges;
            }
            
            _context.Workflows.Add(workflow);
            
            // Add audit log
            var auditLog = new WorkflowAuditLog
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflow.Id,
                Action = "Created",
                Timestamp = DateTime.UtcNow,
                UserId = "system"
            };
            _context.WorkflowAuditLogs.Add(auditLog);
            
            await _context.SaveChangesAsync();
            
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
        var workflow = await _context.Workflows
            .Include(w => w.Nodes)
            .Include(w => w.Edges)
            .Include(w => w.Versions)
            .FirstOrDefaultAsync(w => w.Id == id);
        
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
        var workflows = await _context.Workflows
            .Include(w => w.Versions)
            .OrderByDescending(w => w.ModifiedDate)
            .ToListAsync();
        
        return workflows.Select(w => new WorkflowListItemDTO
        {
            Id = w.Id,
            Name = w.Name,
            Description = w.Description,
            CreatedDate = w.CreatedDate,
            ModifiedDate = w.ModifiedDate,
            IsPublished = w.IsPublished,
            VersionCount = w.Versions.Count
        }).ToList();
    }
    
    /// <summary>
    /// Updates a workflow
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<WorkflowDTO>> UpdateWorkflow(Guid id, [FromBody] UpdateWorkflowRequest request)
    {
        try
        {
            var workflow = await _context.Workflows
                .Include(w => w.Nodes)
                .Include(w => w.Edges)
                .FirstOrDefaultAsync(w => w.Id == id);
            
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
                
                // Remove old nodes and edges
                _context.WorkflowNodes.RemoveRange(workflow.Nodes);
                _context.WorkflowEdges.RemoveRange(workflow.Edges);
                
                // Compile new graph
                var (nodes, edges) = _compilerService.CompileGraph(request.GraphJson, workflow.Id);
                workflow.Nodes = nodes;
                workflow.Edges = edges;
            }
            
            workflow.ModifiedDate = DateTime.UtcNow;
            workflow.ModifiedBy = "system"; // TODO: Get from authentication
            
            // Add audit log
            var auditLog = new WorkflowAuditLog
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflow.Id,
                Action = "Updated",
                Timestamp = DateTime.UtcNow,
                UserId = "system"
            };
            _context.WorkflowAuditLogs.Add(auditLog);
            
            await _context.SaveChangesAsync();
            
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
        var workflow = await _context.Workflows.FindAsync(id);
        
        if (workflow == null)
        {
            return NotFound();
        }
        
        _context.Workflows.Remove(workflow);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted workflow {WorkflowId}", id);
        
        return NoContent();
    }
    
    /// <summary>
    /// Validates a workflow
    /// </summary>
    [HttpPost("{id}/validate")]
    public async Task<ActionResult<ValidationResult>> ValidateWorkflow(Guid id)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Nodes)
            .Include(w => w.Edges)
            .FirstOrDefaultAsync(w => w.Id == id);
        
        if (workflow == null)
        {
            return NotFound();
        }
        
        var validationResult = _validationService.ValidateWorkflow(workflow.Nodes.ToList(), workflow.Edges.ToList());
        
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
