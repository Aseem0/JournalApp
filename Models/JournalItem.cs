using System.Text.Json;

namespace JournalApp.Models;

/// <summary>
/// Represents a single journal entry in the application.
/// </summary>
public class JournalItem
{
    /// <summary>
    /// Unique identifier for the journal entry (auto-incremented by database).
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// The date the journal entry refers to (calendar date).
    /// </summary>
    public DateTime EntryDate { get; set; }
    
    /// <summary>
    /// The main HTML content of the journal entry, typically captured from the rich text editor.
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// The primary mood selected for this entry (e.g., "Happy", "Calm").
    /// </summary>
    public string PrimaryMood { get; set; } = string.Empty;
    
    /// <summary>
    /// Stores up to two secondary moods as a JSON serialized string for database storage.
    /// </summary>
    public string SecondaryMoods { get; set; } = "[]";
    
    /// <summary>
    /// Stores associated tags as a JSON serialized string for database storage.
    /// </summary>
    public string Tags { get; set; } = "[]";
    
    /// <summary>
    /// The exact timestamp when this entry was first created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Helper property to work with Tags as a List of strings.
    /// Handles serialization/deserialization automatically.
    /// </summary>
    public List<string> TagList
    {
        get => string.IsNullOrWhiteSpace(Tags)
            ? []
            : JsonSerializer.Deserialize<List<string>>(Tags) ?? [];

        set => Tags = JsonSerializer.Serialize(value);
    }

    /// <summary>
    /// Helper property to work with SecondaryMoods as a List of strings.
    /// Handles serialization/deserialization automatically.
    /// </summary>
    public List<string> SecondaryMoodList
    {
        get => string.IsNullOrWhiteSpace(SecondaryMoods)
            ? []
            : JsonSerializer.Deserialize<List<string>>(SecondaryMoods) ?? [];

        set => SecondaryMoods = JsonSerializer.Serialize(value);
    }
}
