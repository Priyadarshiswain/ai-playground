using MongoDB.Driver;
using CodeAnalyser.Models;

namespace CodeAnalyser.Services;

public class MongoMethodStore
{
    private readonly IMongoCollection<MethodDocument> _collection;

    public MongoMethodStore(string connectionString, string databaseName = "sk_playground")
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _collection = database.GetCollection<MethodDocument>("methods");

        // Index on FilePath + MethodName for fast lookups
        var indexKeys = Builders<MethodDocument>.IndexKeys
            .Ascending(x => x.FilePath)
            .Ascending(x => x.MethodName);
        _collection.Indexes.CreateOne(new CreateIndexModel<MethodDocument>(indexKeys));
    }

    public async Task UpsertMethodAsync(MethodDocument doc)
    {
        var existing = await _collection
            .Find(x => x.FilePath == doc.FilePath && x.MethodName == doc.MethodName)
            .FirstOrDefaultAsync();

        if (existing == null)
        {
            await _collection.InsertOneAsync(doc);
            return;
        }

        // File hasn't changed — skip
        if (existing.FileHash == doc.FileHash)
            return;

        // File changed — archive current state to history
        existing.History.Add(new MethodHistory
        {
            FileHash = existing.FileHash,
            CyclomaticComplexity = existing.CyclomaticComplexity,
            AiDescription = existing.AiDescription,
            RecordedAt = DateTime.UtcNow
        });

        doc.Id = existing.Id;
        doc.CreatedAt = existing.CreatedAt;
        doc.History = existing.History;
        doc.UpdatedAt = DateTime.UtcNow;

        await _collection.ReplaceOneAsync(x => x.Id == existing.Id, doc);
    }

    public async Task<MethodDocument?> GetMethodAsync(string filePath, string methodName)
    {
        return await _collection
            .Find(x => x.FilePath == filePath && x.MethodName == methodName)
            .FirstOrDefaultAsync();
    }
}