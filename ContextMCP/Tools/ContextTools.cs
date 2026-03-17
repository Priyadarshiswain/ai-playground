using ModelContextProtocol.Server;
using System.ComponentModel;
using ContextMCP.Services;
using ContextMCP.Models;

namespace ContextMCP.Tools;

[McpServerToolType]
public class ContextTools(ContextStore store)
{
    [McpServerTool, Description("Save context from current session to MongoDB")]
    public async Task<string> save_context(
        [Description("Project name e.g. sk-playground")] string project,
        [Description("What we worked on this session")] string topic,
        [Description("Key decisions made, comma separated")] string decisions,
        [Description("Tasks completed this session, comma separated")] string completed,
        [Description("Tasks still pending, comma separated")] string pending,
        [Description("The single next action to take")] string next_action,
        [Description("Brief raw summary of the session")] string summary)
    {
        var session = new ContextSession
        {
            Project = project,
            Topic = topic,
            DecisionsMade = decisions.Split(',', StringSplitOptions.TrimEntries).ToList(),
            TasksCompleted = completed.Split(',', StringSplitOptions.TrimEntries).ToList(),
            TasksPending = pending.Split(',', StringSplitOptions.TrimEntries).ToList(),
            NextAction = next_action,
            RawSummary = summary,
            SavedAt = DateTime.UtcNow
        };

        await store.SaveSessionAsync(session);
        return $"Context saved for project '{project}'. Next action: {next_action}";
    }

    [McpServerTool, Description("Load context for a project from MongoDB")]
    public async Task<string> load_context(
        [Description("Project name e.g. sk-playground")] string project)
    {
        return await store.LoadContextAsync(project);
    }
}