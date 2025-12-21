using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using JournalApp.Models;

namespace JournalApp.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath;

        public DatabaseService()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _dbPath = System.IO.Path.Combine(folder, "journal.db");

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS JournalItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EntryDate TEXT NOT NULL,
                Content TEXT NOT NULL,
                PrimaryMood TEXT,
                SecondaryMoods TEXT,
                Tags TEXT,
                CreatedAt TEXT NOT NULL
            );
            ";

            cmd.ExecuteNonQuery();
        }

        // ================= CREATE =================
        public void SaveEntry(JournalItem entry)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
            @"
            INSERT INTO JournalItems
            (EntryDate, Content, PrimaryMood, SecondaryMoods, Tags, CreatedAt)
            VALUES ($entryDate, $content, $mood, $secondary, $tags, $createdAt);
            ";

            cmd.Parameters.AddWithValue("$entryDate", entry.EntryDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$content", entry.Content);
            cmd.Parameters.AddWithValue("$mood", entry.PrimaryMood ?? "");
            cmd.Parameters.AddWithValue("$secondary", entry.SecondaryMoods ?? "");
            cmd.Parameters.AddWithValue("$tags", entry.Tags ?? "");
            cmd.Parameters.AddWithValue("$createdAt", entry.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

            cmd.ExecuteNonQuery();
        }

        // ================= READ ALL =================
        public List<JournalItem> GetEntries()
        {
            var list = new List<JournalItem>();

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM JournalItems ORDER BY EntryDate DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new JournalItem
                {
                    Id = reader.GetInt32(0),
                    EntryDate = DateTime.Parse(reader.GetString(1)),
                    Content = reader.GetString(2),
                    PrimaryMood = reader.GetString(3),
                    SecondaryMoods = reader.GetString(4),
                    Tags = reader.GetString(5),
                    CreatedAt = DateTime.Parse(reader.GetString(6))
                });
            }

            return list;
        }

        // ================= READ BY ID =================
        public JournalItem? GetEntryById(int id)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM JournalItems WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new JournalItem
                {
                    Id = reader.GetInt32(0),
                    EntryDate = DateTime.Parse(reader.GetString(1)),
                    Content = reader.GetString(2),
                    PrimaryMood = reader.GetString(3),
                    SecondaryMoods = reader.GetString(4),
                    Tags = reader.GetString(5),
                    CreatedAt = DateTime.Parse(reader.GetString(6))
                };
            }

            return null;
        }

        // ================= UPDATE =================
        public void UpdateEntry(JournalItem entry)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
            @"
            UPDATE JournalItems
            SET Content = $content,
                PrimaryMood = $mood,
                SecondaryMoods = $secondary,
                Tags = $tags
            WHERE Id = $id;
            ";

            cmd.Parameters.AddWithValue("$content", entry.Content);
            cmd.Parameters.AddWithValue("$mood", entry.PrimaryMood ?? "");
            cmd.Parameters.AddWithValue("$secondary", entry.SecondaryMoods ?? "");
            cmd.Parameters.AddWithValue("$tags", entry.Tags ?? "");
            cmd.Parameters.AddWithValue("$id", entry.Id);

            cmd.ExecuteNonQuery();
        }

        // ================= DELETE =================
        // DELETE
        public async Task DeleteEntryAsync(int id)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM JournalItems WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);

            await cmd.ExecuteNonQueryAsync();
        }


    }
}
