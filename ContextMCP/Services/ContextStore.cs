using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ContextMCP.Models;

namespace ContextMCP.Services;

public class ContextStore
{
    private readonly IMongoCollection<BsonDocument> _sessions;
    private readonly IMongoCollection<BsonDocument> _projects;

    public ContextStore(string connectionString, string databaseName = "sk_playground")
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _sessions = database.GetCollection<BsonDocument>("context_sessions");
        _projects = database.GetCollection<BsonDocument>("context_projects");
    }

    public async Task SaveSessionAsync(ContextSession session)
    {
        var doc = new BsonDocument
        {
            ["project"] = session.Project,
            ["topic"] = session.Topic,
            ["decisionsMade"] = new BsonArray(session.DecisionsMade),
            ["tasksCompleted"] = new BsonArray(session.TasksCompleted),
            ["tasksPending"] = new BsonArray(session.TasksPending),
            ["nextAction"] = session.NextAction,
            ["rawSummary"] = session.RawSummary,
            ["savedAt"] = session.SavedAt
        };

        await _sessions.InsertOneAsync(doc);
    }

    public async Task<string> LoadContextAsync(string project)
    {
        // Get project summary
        var projectDoc = await _projects
            .Find(Builders<BsonDocument>.Filter.Eq("project", project))
            .FirstOrDefaultAsync();

        // Get last 3 sessions
        var sessions = await _sessions
            .Find(Builders<BsonDocument>.Filter.Eq("project", project))
            .Sort(Builders<BsonDocument>.Sort.Descending("savedAt"))
            .Limit(3)
            .ToListAsync();

        if (projectDoc == null && sessions.Count == 0)
            return $"No context found for project '{project}'.";

        var sb = new System.Text.StringBuilder();

        if (projectDoc != null)
        {
            sb.AppendLine("## Project Context");
            sb.AppendLine($"**Project:** {projectDoc["project"]}");
            sb.AppendLine($"**Stack:** {projectDoc["stack"]}");
            sb.AppendLine($"**Phase:** {projectDoc["phase"]}");
            sb.AppendLine($"**System Prompt:** {projectDoc["systemPrompt"]}");
            sb.AppendLine();
        }

        if (sessions.Count > 0)
        {
            sb.AppendLine("## Recent Sessions");
            foreach (var s in sessions)
            {
                sb.AppendLine($"### {s["savedAt"].ToUniversalTime():yyyy-MM-dd} — {s["topic"]}");
                sb.AppendLine($"**Next action:** {s["nextAction"]}");
                sb.AppendLine($"**Decisions:** {string.Join(", ", s["decisionsMade"].AsBsonArray)}");
                sb.AppendLine($"**Completed:** {string.Join(", ", s["tasksCompleted"].AsBsonArray)}");
                sb.AppendLine($"**Pending:** {string.Join(", ", s["tasksPending"].AsBsonArray)}");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    public async Task UpsertProjectAsync(ContextProject project)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("project", project.Project);
        var doc = new BsonDocument
        {
            ["project"] = project.Project,
            ["stack"] = project.Stack,
            ["phase"] = project.Phase,
            ["systemPrompt"] = project.SystemPrompt,
            ["keyDecisions"] = new BsonArray(project.KeyDecisions),
            ["updatedAt"] = project.UpdatedAt
        };

        await _projects.ReplaceOneAsync(filter, doc, new ReplaceOptions { IsUpsert = true });
    }
}