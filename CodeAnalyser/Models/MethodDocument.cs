using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CodeAnalyser.Models;

public class MethodDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string FilePath { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
    public string NamespaceName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string MethodName { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;

    public int CyclomaticComplexity { get; set; }
    public int LineCount { get; set; }
    public string ReturnType { get; set; } = string.Empty;
    public List<string> Parameters { get; set; } = [];
    public List<string> UsedTypes { get; set; } = [];
    public List<string> CalledMethods { get; set; } = [];
    public bool HasTodo { get; set; }
    public List<string> TodoComments { get; set; } = [];

    public string AiDescription { get; set; } = string.Empty;
    public string AiRiskLevel { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<MethodHistory> History { get; set; } = [];
}

public class MethodHistory
{
    public string FileHash { get; set; } = string.Empty;
    public int CyclomaticComplexity { get; set; }
    public string AiDescription { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}