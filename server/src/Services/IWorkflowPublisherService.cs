using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

/// <summary>
/// Interface for workflow publisher service
/// </summary>
public interface IWorkflowPublisherService
{
    Task<WorkflowVersion> PublishVersionAsync(Guid workflowId, string versionNumber, string publishedBy, string? releaseNotes = null);
    Task<WorkflowVersion?> GetActiveVersionAsync(Guid workflowId);
    Task<List<WorkflowVersion>> GetVersionsAsync(Guid workflowId);
}
