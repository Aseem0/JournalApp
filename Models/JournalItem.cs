using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JournalApp.Models
{
    public class JournalItem
    {
        public int Id { get; set; }
        public DateTime EntryDate { get; set; }
        public string Content { get; set; }
        public string PrimaryMood { get; set; }
        public string SecondaryMoods { get; set; } // JSON string
        public string Tags { get; set; } = "[]"; // JSON string
        public DateTime CreatedAt { get; set; }

        public List<string> TagList
        {
            get => string.IsNullOrWhiteSpace(Tags)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(Tags)!;

            set => Tags = JsonSerializer.Serialize(value);
        }
    }
}
