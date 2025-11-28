using WorkflowEngine.Models;

namespace WorkflowEngine.Data;

/// <summary>
/// Interface for workflow repository operations
/// </summary>
public interface IWorkflowRepository
{
    Task<Workflow> CreateAsync(Workflow workflow);
    Task<Workflow?> GetByIdAsync(Guid id);
    Task<List<Workflow>> GetAllAsync();
    Task<bool> UpdateAsync(Workflow workflow);
    Task<bool> DeleteAsync(Guid id);
    Task<int> GetVersionCountAsync(Guid workflowId);
}
