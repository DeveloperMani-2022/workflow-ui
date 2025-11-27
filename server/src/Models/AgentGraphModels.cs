using System.Text.Json.Serialization;

namespace WorkflowEngine.Models;

/// <summary>
/// Represents the flat dictionary structure of the AI Agent Builder
/// Keys are node IDs (or "start"), values are the node definitions
/// </summary>
public class AgentGraphDefinition : Dictionary<string, AgentGraphNode>
{
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TriggerNode), typeDiscriminator: "trigger")]
[JsonDerivedType(typeof(AgentNode), typeDiscriminator: "agent")]
[JsonDerivedType(typeof(ActionNode), typeDiscriminator: "action")]
[JsonDerivedType(typeof(ConditionNode), typeDiscriminator: "condition")]
[JsonDerivedType(typeof(NoteNode), typeDiscriminator: "note")]
public class AgentGraphNode
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public List<GraphConnection> To { get; set; } = new();
    
    // Common optional properties
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }
}

public class TriggerNode : AgentGraphNode
{
    [JsonPropertyName("triggerKey")]
    public string TriggerKey { get; set; } = string.Empty;
}

public class AgentNode : AgentGraphNode
{
    [JsonPropertyName("skills")]
    public Dictionary<string, Skill> Skills { get; set; } = new();
}

public class ActionNode : AgentGraphNode
{
    [JsonPropertyName("actionKey")]
    public string ActionKey { get; set; } = string.Empty;
}

public class ConditionNode : AgentGraphNode
{
    // Conditions often use the 'prompt' in the connection to define the branch logic
}

public class NoteNode : AgentGraphNode
{
    [JsonPropertyName("markdown")]
    public string Markdown { get; set; } = string.Empty;
}

public class GraphConnection
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; } // Used for edge labels/conditions
}

public class Skill
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
}
