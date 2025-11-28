using WorkflowEngine.Models;

namespace WorkflowEngine.Data;

/// <summary>
/// Interface for workflow version repository operations
/// </summary>
public interface IWorkflowVersionRepository
{
    Task<WorkflowVersion> CreateAsync(WorkflowVersion version);
    Task<WorkflowVersion?> GetActiveVersionAsync(Guid workflowId);
    Task<List<WorkflowVersion>> GetByWorkflowIdAsync(Guid workflowId);
    Task<bool> VersionExistsAsync(Guid workflowId, string versionNumber);
    Task DeactivateAllVersionsAsync(Guid workflowId);
}
