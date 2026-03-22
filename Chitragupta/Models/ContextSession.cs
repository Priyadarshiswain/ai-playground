namespace Chitragupta.Models;

public class ContextSession
{
    public string? Id { get; set; }
    public string Project { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public List<string> DecisionsMade { get; set; } = [];
    public List<string> TasksCompleted { get; set; } = [];
    public List<string> TasksPending { get; set; } = [];
    public string NextAction { get; set; } = string.Empty;
    public string RawSummary { get; set; } = string.Empty;
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}

public class ContextProject
{
    public string? Id { get; set; }
    public string Project { get; set; } = string.Empty;
    public string Stack { get; set; } = string.Empty;
    public string Phase { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public List<string> KeyDecisions { get; set; } = [];
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}