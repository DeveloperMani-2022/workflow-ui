using WorkflowEngine.Data;
using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

/// <summary>
/// Service for publishing and managing workflow versions
/// </summary>
public class WorkflowPublisherService
{
    private readonly WorkflowRepository _workflowRepository;
    private readonly WorkflowVersionRepository _versionRepository;
    private readonly AuditLogRepository _auditLogRepository;
    
    public WorkflowPublisherService(
        WorkflowRepository workflowRepository,
        WorkflowVersionRepository versionRepository,
        AuditLogRepository auditLogRepository)
    {
        _workflowRepository = workflowRepository;
        _versionRepository = versionRepository;
        _auditLogRepository = auditLogRepository;
    }
    
    /// <summary>
    /// Publishes a new version of a workflow
    /// </summary>
    public async Task<WorkflowVersion> PublishVersionAsync(Guid workflowId, string versionNumber, string publishedBy, string? releaseNotes = null)
    {
        var workflow = await _workflowRepository.GetByIdAsync(workflowId);
        
        if (workflow == null)
        {
            throw new InvalidOperationException("Workflow not found");
        }
        
        // Check if version already exists
        var versionExists = await _versionRepository.VersionExistsAsync(workflowId, versionNumber);
        if (versionExists)
        {
            throw new InvalidOperationException($"Version {versionNumber} already exists");
        }
        
        // Deactivate all previous versions
        await _versionRepository.DeactivateAllVersionsAsync(workflowId);
        
        // Create new version
        var newVersion = new WorkflowVersion
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
            VersionNumber = versionNumber,
            GraphJson = workflow.GraphJson,
            PublishedDate = DateTime.UtcNow,
            PublishedBy = publishedBy,
            ReleaseNotes = releaseNotes,
            IsActive = true
        };
        
        await _versionRepository.CreateAsync(newVersion);
        
        // Mark workflow as published
        workflow.IsPublished = true;
        workflow.ModifiedDate = DateTime.UtcNow;
        workflow.ModifiedBy = publishedBy;
        await _workflowRepository.UpdateAsync(workflow);
        
        // Add audit log
        var auditLog = new WorkflowAuditLog
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
            Action = "Published",
            Timestamp = DateTime.UtcNow,
            UserId = publishedBy,
            Details = $"Published version {versionNumber}"
        };
        
        await _auditLogRepository.CreateAsync(auditLog);
        
        return newVersion;
    }
    
    /// <summary>
    /// Gets the active version of a workflow
    /// </summary>
    public async Task<WorkflowVersion?> GetActiveVersionAsync(Guid workflowId)
    {
        return await _versionRepository.GetActiveVersionAsync(workflowId);
    }
    
    /// <summary>
    /// Gets all versions of a workflow
    /// </summary>
    public async Task<List<WorkflowVersion>> GetVersionsAsync(Guid workflowId)
    {
        return await _versionRepository.GetByWorkflowIdAsync(workflowId);
    }
}
