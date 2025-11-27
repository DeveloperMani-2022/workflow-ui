using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Data;
using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

/// <summary>
/// Service for publishing and managing workflow versions
/// </summary>
public class WorkflowPublisherService
{
    private readonly ApplicationDbContext _context;
    
    public WorkflowPublisherService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Publishes a new version of a workflow
    /// </summary>
    public async Task<WorkflowVersion> PublishVersionAsync(Guid workflowId, string versionNumber, string publishedBy, string? releaseNotes = null)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Versions)
            .FirstOrDefaultAsync(w => w.Id == workflowId);
        
        if (workflow == null)
        {
            throw new InvalidOperationException("Workflow not found");
        }
        
        // Check if version already exists
        var existingVersion = workflow.Versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
        if (existingVersion != null)
        {
            throw new InvalidOperationException($"Version {versionNumber} already exists");
        }
        
        // Deactivate all previous versions
        foreach (var version in workflow.Versions)
        {
            version.IsActive = false;
        }
        
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
        
        _context.WorkflowVersions.Add(newVersion);
        
        // Mark workflow as published
        workflow.IsPublished = true;
        workflow.ModifiedDate = DateTime.UtcNow;
        workflow.ModifiedBy = publishedBy;
        
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
        
        _context.WorkflowAuditLogs.Add(auditLog);
        
        await _context.SaveChangesAsync();
        
        return newVersion;
    }
    
    /// <summary>
    /// Gets the active version of a workflow
    /// </summary>
    public async Task<WorkflowVersion?> GetActiveVersionAsync(Guid workflowId)
    {
        return await _context.WorkflowVersions
            .FirstOrDefaultAsync(v => v.WorkflowId == workflowId && v.IsActive);
    }
    
    /// <summary>
    /// Gets all versions of a workflow
    /// </summary>
    public async Task<List<WorkflowVersion>> GetVersionsAsync(Guid workflowId)
    {
        return await _context.WorkflowVersions
            .Where(v => v.WorkflowId == workflowId)
            .OrderByDescending(v => v.PublishedDate)
            .ToListAsync();
    }
}
