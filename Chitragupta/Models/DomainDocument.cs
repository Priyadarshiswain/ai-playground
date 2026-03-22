public class DomainDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    public string UserId { get; set; } = default!;
    public string Domain { get; set; } = default!; // tech/career/finance/personal/learning

    public List<ProjectEntry> Projects { get; set; } = new();

    [BsonRepresentation(BsonType.String)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string UpdatedBy { get; set; } = default!;
}

public class ProjectEntry
{
    public string Name { get; set; } = default!;
    public string Status { get; set; } = "active"; // active/paused/done
    public List<string> Stack { get; set; } = new();
    public string Summary { get; set; } = default!;
}