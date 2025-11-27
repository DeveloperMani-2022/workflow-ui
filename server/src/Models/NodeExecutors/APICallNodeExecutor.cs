using System.Text;
using System.Text.Json;

namespace WorkflowEngine.Models.NodeExecutors;

/// <summary>
/// Executor for API call nodes that make HTTP requests
/// </summary>
public class APICallNodeExecutor : INodeExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    public string NodeType => "APICallNode";
    
    public APICallNodeExecutor(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<NodeResult> ExecuteAsync(NodeContext context)
    {
        try
        {
            // Extract configuration
            var apiUrl = GetConfigValue<string>(context.NodeConfig, "apiUrl", "");
            var method = GetConfigValue<string>(context.NodeConfig, "method", "GET").ToUpper();
            var headers = GetConfigValue<Dictionary<string, string>>(context.NodeConfig, "headers", new());
            var bodyMapping = GetConfigValue<Dictionary<string, object>>(context.NodeConfig, "bodyMapping", new());
            var responseStateKey = GetConfigValue<string>(context.NodeConfig, "responseStateKey", "apiResponse");
            
            // Process URL template
            var processedUrl = ProcessTemplate(apiUrl, context.StateVariables);
            
            // Create HTTP client
            var httpClient = _httpClientFactory.CreateClient();
            
            // Add headers
            foreach (var header in headers)
            {
                var headerValue = ProcessTemplate(header.Value, context.StateVariables);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, headerValue);
            }
            
            // Prepare request
            HttpResponseMessage response;
            
            if (method == "GET")
            {
                response = await httpClient.GetAsync(processedUrl);
            }
            else if (method == "POST" || method == "PUT" || method == "PATCH")
            {
                // Build request body from mapping
                var requestBody = BuildRequestBody(bodyMapping, context.StateVariables);
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                response = method switch
                {
                    "POST" => await httpClient.PostAsync(processedUrl, content),
                    "PUT" => await httpClient.PutAsync(processedUrl, content),
                    "PATCH" => await httpClient.PatchAsync(processedUrl, content),
                    _ => throw new InvalidOperationException($"Unsupported method: {method}")
                };
            }
            else if (method == "DELETE")
            {
                response = await httpClient.DeleteAsync(processedUrl);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported HTTP method: {method}");
            }
            
            // Read response
            var responseContent = await response.Content.ReadAsStringAsync();
            object? responseData = null;
            
            // Try to parse as JSON
            try
            {
                responseData = JsonSerializer.Deserialize<object>(responseContent);
            }
            catch
            {
                responseData = responseContent;
            }
            
            var result = new NodeResult
            {
                Success = response.IsSuccessStatusCode,
                Output = new Dictionary<string, object>
                {
                    ["statusCode"] = (int)response.StatusCode,
                    ["response"] = responseData ?? "",
                    ["url"] = processedUrl,
                    ["method"] = method
                },
                StateUpdates = new Dictionary<string, object>
                {
                    [responseStateKey] = responseData ?? ""
                }
            };
            
            if (!response.IsSuccessStatusCode)
            {
                result.ErrorMessage = $"API call failed with status {response.StatusCode}: {responseContent}";
            }
            
            return result;
        }
        catch (Exception ex)
        {
            return NodeResult.FailureResult($"API call node execution failed: {ex.Message}");
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
    
    private Dictionary<string, object> BuildRequestBody(Dictionary<string, object> mapping, Dictionary<string, object> stateVariables)
    {
        var body = new Dictionary<string, object>();
        
        foreach (var kvp in mapping)
        {
            var value = kvp.Value;
            
            // If value is a template string, process it
            if (value is string strValue && strValue.StartsWith("{") && strValue.EndsWith("}"))
            {
                var varName = strValue.Trim('{', '}');
                if (stateVariables.TryGetValue(varName, out var stateValue))
                {
                    body[kvp.Key] = stateValue;
                }
                else
                {
                    body[kvp.Key] = value;
                }
            }
            else
            {
                body[kvp.Key] = value;
            }
        }
        
        return body;
    }
}
