using System.Text.Json;

namespace WorkflowEngine.Models.NodeExecutors;

/// <summary>
/// Executor for message nodes that display information to users
/// </summary>
public class MessageNodeExecutor : INodeExecutor
{
    public string NodeType => "MessageNode";
    
    public Task<NodeResult> ExecuteAsync(NodeContext context)
    {
        try
        {
            // Extract message text from node configuration
            var messageText = GetConfigValue<string>(context.NodeConfig, "messageText", "");
            
            // Support template variables in messages (e.g., "Hello {userName}")
            var processedMessage = ProcessTemplate(messageText, context.StateVariables);
            
            var result = new NodeResult
            {
                Success = true,
                Message = processedMessage,
                Output = new Dictionary<string, object>
                {
                    ["displayedMessage"] = processedMessage,
                    ["timestamp"] = DateTime.UtcNow
                }
            };
            
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            return Task.FromResult(NodeResult.FailureResult($"Message node execution failed: {ex.Message}"));
        }
    }
    
    private T GetConfigValue<T>(Dictionary<string, object> config, string key, T defaultValue)
    {
        if (config.TryGetValue(key, out var value))
        {
            if (value is JsonElement jsonElement)
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText()) ?? defaultValue;
            }
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return defaultValue;
    }
    
    private string ProcessTemplate(string template, Dictionary<string, object> variables)
    {
        var result = template;
        foreach (var kvp in variables)
        {
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? "");
        }
        return result;
    }
}
