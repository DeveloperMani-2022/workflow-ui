using System.Text.Json;
using System.Text.RegularExpressions;

namespace WorkflowEngine.Models.NodeExecutors;

/// <summary>
/// Executor for question nodes that prompt users for input
/// </summary>
public class QuestionNodeExecutor : INodeExecutor
{
    public string NodeType => "QuestionNode";
    
    public Task<NodeResult> ExecuteAsync(NodeContext context)
    {
        try
        {
            // Extract configuration
            var promptText = GetConfigValue<string>(context.NodeConfig, "promptText", "Please provide input:");
            var stateKey = GetConfigValue<string>(context.NodeConfig, "stateKey", "userResponse");
            var validationRules = GetConfigValue<Dictionary<string, object>>(context.NodeConfig, "validationRules", new());
            
            // Process template in prompt
            var processedPrompt = ProcessTemplate(promptText, context.StateVariables);
            
            // If user input is provided, validate and store it
            if (context.UserInput != null)
            {
                var inputValue = context.UserInput.ToString() ?? "";
                
                // Validate input
                var validationResult = ValidateInput(inputValue, validationRules);
                if (!validationResult.IsValid)
                {
                    return Task.FromResult(new NodeResult
                    {
                        Success = false,
                        RequiresUserInput = true,
                        Message = processedPrompt,
                        ErrorMessage = validationResult.ErrorMessage
                    });
                }
                
                // Store validated input in state
                var result = new NodeResult
                {
                    Success = true,
                    StateUpdates = new Dictionary<string, object>
                    {
                        [stateKey] = inputValue
                    },
                    Output = new Dictionary<string, object>
                    {
                        ["question"] = processedPrompt,
                        ["answer"] = inputValue,
                        ["stateKey"] = stateKey
                    }
                };
                
                return Task.FromResult(result);
            }
            
            // No input yet, request it from user
            return Task.FromResult(NodeResult.UserInputRequired(processedPrompt));
        }
        catch (Exception ex)
        {
            return Task.FromResult(NodeResult.FailureResult($"Question node execution failed: {ex.Message}"));
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
    
    private (bool IsValid, string? ErrorMessage) ValidateInput(string input, Dictionary<string, object> rules)
    {
        // Required validation
        if (rules.TryGetValue("required", out var requiredObj) && requiredObj is bool required && required)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return (false, "This field is required.");
            }
        }
        
        // Min length validation
        if (rules.TryGetValue("minLength", out var minLengthObj) && minLengthObj is JsonElement minLengthElement)
        {
            var minLength = minLengthElement.GetInt32();
            if (input.Length < minLength)
            {
                return (false, $"Input must be at least {minLength} characters.");
            }
        }
        
        // Max length validation
        if (rules.TryGetValue("maxLength", out var maxLengthObj) && maxLengthObj is JsonElement maxLengthElement)
        {
            var maxLength = maxLengthElement.GetInt32();
            if (input.Length > maxLength)
            {
                return (false, $"Input must not exceed {maxLength} characters.");
            }
        }
        
        // Pattern validation (regex)
        if (rules.TryGetValue("pattern", out var patternObj) && patternObj is string pattern)
        {
            if (!Regex.IsMatch(input, pattern))
            {
                var patternMessage = rules.TryGetValue("patternMessage", out var msgObj) && msgObj is string msg 
                    ? msg 
                    : "Input does not match the required format.";
                return (false, patternMessage);
            }
        }
        
        return (true, null);
    }
}
