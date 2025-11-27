using System.Text.Json;
using System.Text.RegularExpressions;

namespace WorkflowEngine.Models.NodeExecutors;

/// <summary>
/// Executor for condition nodes that evaluate expressions for branching
/// </summary>
public class ConditionNodeExecutor : INodeExecutor
{
    public string NodeType => "ConditionNode";
    
    public Task<NodeResult> ExecuteAsync(NodeContext context)
    {
        try
        {
            // Extract configuration
            var conditionExpressions = GetConfigValue<List<ConditionBranch>>(context.NodeConfig, "branches", new());
            
            // Evaluate each condition in order
            foreach (var branch in conditionExpressions)
            {
                if (EvaluateCondition(branch.Condition, context.StateVariables))
                {
                    var result = new NodeResult
                    {
                        Success = true,
                        NextPort = branch.Port,
                        Output = new Dictionary<string, object>
                        {
                            ["evaluatedCondition"] = branch.Condition,
                            ["selectedBranch"] = branch.Label ?? branch.Port,
                            ["result"] = true
                        }
                    };
                    
                    return Task.FromResult(result);
                }
            }
            
            // No condition matched, use default/else branch if available
            var defaultBranch = conditionExpressions.FirstOrDefault(b => b.IsDefault);
            if (defaultBranch != null)
            {
                return Task.FromResult(new NodeResult
                {
                    Success = true,
                    NextPort = defaultBranch.Port,
                    Output = new Dictionary<string, object>
                    {
                        ["selectedBranch"] = "default",
                        ["result"] = false
                    }
                });
            }
            
            return Task.FromResult(NodeResult.FailureResult("No condition matched and no default branch defined."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(NodeResult.FailureResult($"Condition node execution failed: {ex.Message}"));
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
    
    /// <summary>
    /// Evaluates a condition expression against state variables
    /// Supports: ==, !=, >, <, >=, <=, contains, startsWith, endsWith
    /// Example: "age > 18", "status == 'active'", "name contains 'John'"
    /// </summary>
    private bool EvaluateCondition(string condition, Dictionary<string, object> stateVariables)
    {
        // Simple expression parser
        var operators = new[] { "==", "!=", ">=", "<=", ">", "<", "contains", "startsWith", "endsWith" };
        
        foreach (var op in operators)
        {
            if (condition.Contains(op))
            {
                var parts = condition.Split(new[] { op }, StringSplitOptions.TrimEntries);
                if (parts.Length != 2) continue;
                
                var leftValue = GetValue(parts[0].Trim(), stateVariables);
                var rightValue = GetValue(parts[1].Trim().Trim('\'', '"'), stateVariables);
                
                return op switch
                {
                    "==" => AreEqual(leftValue, rightValue),
                    "!=" => !AreEqual(leftValue, rightValue),
                    ">" => CompareNumeric(leftValue, rightValue) > 0,
                    "<" => CompareNumeric(leftValue, rightValue) < 0,
                    ">=" => CompareNumeric(leftValue, rightValue) >= 0,
                    "<=" => CompareNumeric(leftValue, rightValue) <= 0,
                    "contains" => leftValue?.ToString()?.Contains(rightValue?.ToString() ?? "") ?? false,
                    "startsWith" => leftValue?.ToString()?.StartsWith(rightValue?.ToString() ?? "") ?? false,
                    "endsWith" => leftValue?.ToString()?.EndsWith(rightValue?.ToString() ?? "") ?? false,
                    _ => false
                };
            }
        }
        
        // If no operator found, treat as boolean variable
        var boolValue = GetValue(condition.Trim(), stateVariables);
        return boolValue is bool b && b;
    }
    
    private object? GetValue(string expression, Dictionary<string, object> stateVariables)
    {
        // Check if it's a state variable
        if (stateVariables.TryGetValue(expression, out var value))
        {
            return value;
        }
        
        // Check if it's a literal value
        if (int.TryParse(expression, out var intValue))
            return intValue;
        
        if (double.TryParse(expression, out var doubleValue))
            return doubleValue;
        
        if (bool.TryParse(expression, out var boolValue))
            return boolValue;
        
        // Return as string
        return expression;
    }
    
    private bool AreEqual(object? left, object? right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        
        return left.ToString() == right.ToString();
    }
    
    private int CompareNumeric(object? left, object? right)
    {
        var leftNum = Convert.ToDouble(left);
        var rightNum = Convert.ToDouble(right);
        return leftNum.CompareTo(rightNum);
    }
}

/// <summary>
/// Represents a conditional branch
/// </summary>
public class ConditionBranch
{
    public string Condition { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string? Label { get; set; }
    public bool IsDefault { get; set; }
}
