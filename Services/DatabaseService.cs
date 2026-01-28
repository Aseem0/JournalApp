using Microsoft.Data.Sqlite;
using JournalApp.Models;

namespace JournalApp.Services;

/// <summary>
/// Service responsible for all database operations, managing journal entries in a SQLite database.
/// </summary>
public class DatabaseService
{
    private readonly string _dbPath;

    public DatabaseService()
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _dbPath = Path.Combine(folder, "journal.db");

        InitializeDatabase();
    }

    /// <summary>
    /// Initializes the SQLite database and creates the necessary tables if they don't exist.
    /// </summary>
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

    /// <summary>
    /// Saves a new journal entry to the database.
    /// </summary>
    /// <param name="entry">The entry to save.</param>
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

    /// <summary>
    /// Retrieves all journal entries ordered by date (newest first).
    /// </summary>
    /// <returns>A collection of all journal entries.</returns>
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

    /// <summary>
    /// Retrieves a specific journal entry by its unique ID.
    /// </summary>
    /// <param name="id">The ID of the entry to find.</param>
    /// <returns>The found entry, or null if not found.</returns>
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

    /// <summary>
    /// Retrieves a journal entry for a specific calendar date.
    /// </summary>
    /// <param name="date">The date to search for.</param>
    /// <returns>The entry for that date, or null if none exists.</returns>
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

    /// <summary>
    /// Updates an existing journal entry in the database.
    /// </summary>
    /// <param name="entry">The entry with updated values.</param>
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

    /// <summary>
    /// Deletes a specific journal entry from the database.
    /// </summary>
    /// <param name="id">The ID of the entry to delete.</param>
    public async Task DeleteEntryAsync(int id)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM JournalItems WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Permanently deletes all journal entries from the database.
    /// </summary>
    public async Task DeleteAllEntriesAsync()
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM JournalItems";

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Maps a single SQLite database record to a JournalItem model.
    /// </summary>
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
