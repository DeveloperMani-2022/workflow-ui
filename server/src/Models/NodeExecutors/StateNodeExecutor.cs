using System.Text.Json;

namespace WorkflowEngine.Models.NodeExecutors;

/// <summary>
/// Executor for state update nodes that modify workflow state
/// </summary>
public class StateNodeExecutor : INodeExecutor
{
    public string NodeType => "StateUpdateNode";
    
    public Task<NodeResult> ExecuteAsync(NodeContext context)
    {
        try
        {
            // Extract configuration - list of state updates to perform
            var updates = GetConfigValue<Dictionary<string, object>>(context.NodeConfig, "updates", new());
            
            var stateUpdates = new Dictionary<string, object>();
            
            foreach (var update in updates)
            {
                var key = update.Key;
                var value = update.Value;
                
                // Process value if it's a template or expression
                var processedValue = ProcessValue(value, context.StateVariables);
                stateUpdates[key] = processedValue;
            }
            
            var result = new NodeResult
            {
                Success = true,
                StateUpdates = stateUpdates,
                Output = new Dictionary<string, object>
                {
                    ["updatedKeys"] = stateUpdates.Keys.ToList(),
                    ["updateCount"] = stateUpdates.Count
                }
            };
            
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            return Task.FromResult(NodeResult.FailureResult($"State update node execution failed: {ex.Message}"));
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
            if (value is T typedValue)
            {
                return typedValue;
            }
        }
        return defaultValue;
    }
    
    private object ProcessValue(object value, Dictionary<string, object> stateVariables)
    {
        // If value is a string template, process it
        if (value is string strValue)
        {
            // Check if it's a direct variable reference
            if (strValue.StartsWith("{") && strValue.EndsWith("}") && !strValue.Contains(" "))
            {
                var varName = strValue.Trim('{', '}');
                if (stateVariables.TryGetValue(varName, out var stateValue))
                {
                    return stateValue;
                }
            }
            
            // Process as template
            var result = strValue;
            foreach (var kvp in stateVariables)
            {
                result = result.Replace($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? "");
            }
            return result;
        }
        
        return value;
    }
}
