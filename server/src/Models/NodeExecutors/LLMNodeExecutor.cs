using System.Text.Json;

namespace WorkflowEngine.Models.NodeExecutors;

/// <summary>
/// Executor for LLM/Agent function nodes
/// </summary>
public class LLMNodeExecutor : INodeExecutor
{
    public string NodeType => "FunctionNode";
    
    public Task<NodeResult> ExecuteAsync(NodeContext context)
    {
        try
        {
            // Extract configuration
            var instructionPrompt = GetConfigValue<string>(context.NodeConfig, "instructionPrompt", "");
            var inputMapping = GetConfigValue<Dictionary<string, string>>(context.NodeConfig, "inputMapping", new());
            var outputMapping = GetConfigValue<Dictionary<string, string>>(context.NodeConfig, "outputMapping", new());
            var modelName = GetConfigValue<string>(context.NodeConfig, "modelName", "gpt-4");
            
            // Build input context from state variables
            var inputContext = new Dictionary<string, object>();
            foreach (var mapping in inputMapping)
            {
                if (context.StateVariables.TryGetValue(mapping.Value, out var value))
                {
                    inputContext[mapping.Key] = value;
                }
            }
            
            // Process instruction prompt with templates
            var processedPrompt = ProcessTemplate(instructionPrompt, context.StateVariables);
            
            // NOTE: This is a placeholder for actual LLM integration
            // In a real implementation, you would:
            // 1. Call an LLM API (OpenAI, Azure OpenAI, etc.)
            // 2. Pass the processedPrompt and inputContext
            // 3. Parse the response
            // 4. Map outputs to state variables
            
            // For now, we'll simulate a response
            var simulatedResponse = new Dictionary<string, object>
            {
                ["response"] = $"Simulated LLM response for: {processedPrompt}",
                ["model"] = modelName,
                ["timestamp"] = DateTime.UtcNow,
                ["inputContext"] = inputContext
            };
            
            // Map outputs to state variables
            var stateUpdates = new Dictionary<string, object>();
            foreach (var mapping in outputMapping)
            {
                if (simulatedResponse.TryGetValue(mapping.Key, out var value))
                {
                    stateUpdates[mapping.Value] = value;
                }
            }
            
            var result = new NodeResult
            {
                Success = true,
                Output = simulatedResponse,
                StateUpdates = stateUpdates,
                Message = "LLM function executed successfully (simulated)"
            };
            
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            return Task.FromResult(NodeResult.FailureResult($"LLM node execution failed: {ex.Message}"));
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
