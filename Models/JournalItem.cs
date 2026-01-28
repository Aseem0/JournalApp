using System.Text.Json;

namespace JournalApp.Models;

// Represents a single journal entry in the application.
public class JournalItem
{
    // Unique identifier for the journal entry (auto-incremented by database).
    public int Id { get; set; }
    
    // The date the journal entry refers to (calendar date).
    public DateTime EntryDate { get; set; }
    
    // The main HTML content of the journal entry, typically captured from the rich text editor.
    public string Content { get; set; } = string.Empty;
    
    // The primary mood selected for this entry (e.g., "Happy", "Calm").
    public string PrimaryMood { get; set; } = string.Empty;
    
    // Stores up to two secondary moods as a JSON serialized string for database storage.
    public string SecondaryMoods { get; set; } = "[]";
    
    // Stores associated tags as a JSON serialized string for database storage.
    public string Tags { get; set; } = "[]";
    
    // The exact timestamp when this entry was first created.
    public DateTime CreatedAt { get; set; }

    // Helper property to work with Tags as a List of strings.
    // Handles serialization/deserialization automatically.
    public List<string> TagList
    {
        get => string.IsNullOrWhiteSpace(Tags)
            ? []
            : JsonSerializer.Deserialize<List<string>>(Tags) ?? [];

        set => Tags = JsonSerializer.Serialize(value);
    }

    // Helper property to work with SecondaryMoods as a List of strings.
    // Handles serialization/deserialization automatically.
    public List<string> SecondaryMoodList
    {
        get => string.IsNullOrWhiteSpace(SecondaryMoods)
            ? []
            : JsonSerializer.Deserialize<List<string>>(SecondaryMoods) ?? [];

        set => SecondaryMoods = JsonSerializer.Serialize(value);
    }
}
