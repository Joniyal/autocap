namespace AUTOCAP.Core.Models;

/// <summary>
/// SQLite model for storing subtitle sessions.
/// </summary>
[SQLite.Table("SubtitleSessions")]
public class SubtitleSession
{
    [SQLite.PrimaryKey, SQLite.AutoIncrement]
    public int Id { get; set; }

    public string Title { get; set; } = $"Session {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string SubtitleData { get; set; } = string.Empty; // SRT format
    public string AudioSource { get; set; } = "Unknown";
    public int LineCount { get; set; } = 0;
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Database service for storing and retrieving sessions.
/// </summary>
public class SessionDatabaseService
{
    private readonly string _dbPath;
    private SQLite.SQLiteAsyncConnection? _connection;

    public SessionDatabaseService(string dbPath)
    {
        _dbPath = dbPath;
    }

    public async Task InitializeAsync()
    {
        _connection = new SQLite.SQLiteAsyncConnection(_dbPath);
        await _connection.CreateTableAsync<SubtitleSession>();
    }

    public async Task<int> SaveSessionAsync(SubtitleSession session)
    {
        if (_connection == null) throw new InvalidOperationException("Database not initialized");

        session.UpdatedAt = DateTime.UtcNow;
        if (session.Id == 0)
            return await _connection.InsertAsync(session);
        else
            await _connection.UpdateAsync(session);
        return session.Id;
    }

    public async Task<List<SubtitleSession>> GetAllSessionsAsync()
    {
        if (_connection == null) throw new InvalidOperationException("Database not initialized");
        return await _connection.Table<SubtitleSession>().ToListAsync();
    }

    public async Task<SubtitleSession?> GetSessionAsync(int id)
    {
        if (_connection == null) throw new InvalidOperationException("Database not initialized");
        return await _connection.Table<SubtitleSession>().Where(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteSessionAsync(int id)
    {
        if (_connection == null) throw new InvalidOperationException("Database not initialized");
        int result = await _connection.DeleteAsync<SubtitleSession>(id);
        return result > 0;
    }
}
