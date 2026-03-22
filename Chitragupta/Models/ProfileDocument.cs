using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ProfileDocument
{
    [BsonId]
    public string UserId { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string Role { get; set; } = default!;
    public int YearsExperience { get; set; }
    public List<string> Expertise { get; set; } = new();
    public string WorkingStyle { get; set; } = default!;
    public List<string> CurrentGoals { get; set; } = new();
    public List<string> PreferredTools { get; set; } = new();
    public List<string> PreferredPatterns { get; set; } = new();

    [BsonRepresentation(BsonType.String)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string UpdatedBy { get; set; } = default!; // agentId
}