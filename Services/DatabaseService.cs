using Microsoft.Data.Sqlite;
using JournalApp.Models;

namespace JournalApp.Services;

// Service responsible for all database operations, managing journal entries in a SQLite database.
public class DatabaseService
{
    private readonly string _dbPath;

    public DatabaseService()
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _dbPath = Path.Combine(folder, "journal.db");

        InitializeDatabase();
    }

    // Initializes the SQLite database and creates the necessary tables if they don't exist.
    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS JournalItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EntryDate TEXT NOT NULL,
                Content TEXT NOT NULL,
                PrimaryMood TEXT,
                SecondaryMoods TEXT,
                Tags TEXT,
                CreatedAt TEXT NOT NULL
            );";

        command.ExecuteNonQuery();
    }

    // Saves a new journal entry to the database.
    public async Task SaveEntryAsync(JournalItem entry)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO JournalItems (EntryDate, Content, PrimaryMood, SecondaryMoods, Tags, CreatedAt)
            VALUES ($entryDate, $content, $mood, $secondary, $tags, $createdAt);";

        // Map parameters for safety against injection
        command.Parameters.AddWithValue("$entryDate", entry.EntryDate.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$content", entry.Content);
        command.Parameters.AddWithValue("$mood", entry.PrimaryMood ?? string.Empty);
        command.Parameters.AddWithValue("$secondary", entry.SecondaryMoods ?? string.Empty);
        command.Parameters.AddWithValue("$tags", entry.Tags ?? "[]");
        command.Parameters.AddWithValue("$createdAt", entry.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

        await command.ExecuteNonQueryAsync();
    }

    // Retrieves all journal entries ordered by date (newest first).
    public async Task<IEnumerable<JournalItem>> GetAllEntriesAsync()
    {
        var entries = new List<JournalItem>();

        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM JournalItems ORDER BY EntryDate DESC";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            entries.Add(MapToJournalItem(reader));
        }

        return entries;
    }

    // Retrieves a specific journal entry by its unique ID.
    public async Task<JournalItem?> GetEntryByIdAsync(int id)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM JournalItems WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);

        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapToJournalItem(reader) : null;
    }

    // Retrieves a journal entry for a specific calendar date.
    public async Task<JournalItem?> GetEntryByDateAsync(DateTime date)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM JournalItems WHERE EntryDate = $date LIMIT 1";
        command.Parameters.AddWithValue("$date", date.ToString("yyyy-MM-dd"));

        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapToJournalItem(reader) : null;
    }

    // Updates an existing journal entry in the database.
    public async Task UpdateEntryAsync(JournalItem entry)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE JournalItems
            SET Content = $content,
                PrimaryMood = $mood,
                SecondaryMoods = $secondary,
                Tags = $tags
            WHERE Id = $id;";

        command.Parameters.AddWithValue("$content", entry.Content);
        command.Parameters.AddWithValue("$mood", entry.PrimaryMood ?? string.Empty);
        command.Parameters.AddWithValue("$secondary", entry.SecondaryMoods ?? string.Empty);
        command.Parameters.AddWithValue("$tags", entry.Tags ?? "[]");
        command.Parameters.AddWithValue("$id", entry.Id);

        await command.ExecuteNonQueryAsync();
    }

    // Deletes a specific journal entry from the database.
    public async Task DeleteEntryAsync(int id)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM JournalItems WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);

        await command.ExecuteNonQueryAsync();
    }

    // Permanently deletes all journal entries from the database.
    public async Task DeleteAllEntriesAsync()
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM JournalItems";

        await command.ExecuteNonQueryAsync();
    }

    // Maps a single SQLite database record to a JournalItem model.
    private static JournalItem MapToJournalItem(SqliteDataReader reader)
    {
        return new JournalItem
        {
            Id = reader.GetInt32(0),
            EntryDate = DateTime.Parse(reader.GetString(1)),
            Content = reader.GetString(2),
            PrimaryMood = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            SecondaryMoods = reader.IsDBNull(4) ? "[]" : reader.GetString(4),
            Tags = reader.IsDBNull(5) ? "[]" : reader.GetString(5),
            CreatedAt = DateTime.Parse(reader.GetString(6))
        };
    }
}
