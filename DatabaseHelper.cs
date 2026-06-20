using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Represents a single cybersecurity task stored in the database.
    /// </summary>
    public class TaskItem
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime DateCreated { get; set; }
    }

    /// <summary>
    /// Handles all MySQL database operations for tasks and the activity log.
    /// Keeps all SQL/connection logic in one place (Code Optimisation requirement).
    /// </summary>
    public class DatabaseHelper
    {
        // EDIT THIS if your MySQL root password is different
        private const string ConnectionString =
            "Server=localhost;Port=3306;Database=CybersecurityChatbotDB;Uid=root;Pwd=Kinola@2004;";

        /// <summary>
        /// Tests whether the database connection works. Returns true/false
        /// so the app never crashes if MySQL isn't running.
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ---------------- ACTIVITY LOG ----------------

        /// <summary>
        /// Inserts a new entry into the ActivityLog table.
        /// </summary>
        public void AddLogEntry(string description)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            string sql = "INSERT INTO ActivityLog (Description, Timestamp) VALUES (@desc, @ts);";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@desc", description);
            cmd.Parameters.AddWithValue("@ts", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Returns the most recent log entries, newest first, formatted as plain strings.
        /// </summary>
        public List<string> GetRecentLogEntries(int maxCount)
        {
            var entries = new List<string>();
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            string sql = "SELECT Description, Timestamp FROM ActivityLog ORDER BY Timestamp DESC LIMIT @max;";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@max", maxCount);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string desc = reader.GetString("Description");
                DateTime ts = reader.GetDateTime("Timestamp");
                entries.Add($"{ts:dd MMM HH:mm} - {desc}");
            }
            return entries;
        }

        // ---------------- TASKS ----------------

        /// <summary>
        /// Inserts a new task and returns its generated TaskId.
        /// </summary>
        public int AddTask(string title, string description, DateTime? reminderDate)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            string sql = @"INSERT INTO Tasks (Title, Description, ReminderDate, IsCompleted, DateCreated)
                            VALUES (@title, @desc, @reminder, 0, @created);
                            SELECT LAST_INSERT_ID();";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@desc", description);
            cmd.Parameters.AddWithValue("@reminder", reminderDate.HasValue ? (object)reminderDate.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@created", DateTime.Now);
            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Returns all tasks in the database.
        /// </summary>
        public List<TaskItem> GetAllTasks()
        {
            var tasks = new List<TaskItem>();
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            string sql = "SELECT TaskId, Title, Description, ReminderDate, IsCompleted, DateCreated FROM Tasks ORDER BY TaskId;";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tasks.Add(new TaskItem
                {
                    TaskId = reader.GetInt32("TaskId"),
                    Title = reader.GetString("Title"),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description"),
                    ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate")) ? (DateTime?)null : reader.GetDateTime("ReminderDate"),
                    IsCompleted = reader.GetBoolean("IsCompleted"),
                    DateCreated = reader.GetDateTime("DateCreated")
                });
            }
            return tasks;
        }

        /// <summary>
        /// Marks a task as completed. Returns true if a matching task was found and updated.
        /// </summary>
        public bool CompleteTask(int taskId)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            string sql = "UPDATE Tasks SET IsCompleted = 1 WHERE TaskId = @id;";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", taskId);
            int rows = cmd.ExecuteNonQuery();
            return rows > 0;
        }

        /// <summary>
        /// Deletes a task. Returns true if a matching task was found and removed.
        /// </summary>
        public bool DeleteTask(int taskId)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            string sql = "DELETE FROM Tasks WHERE TaskId = @id;";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", taskId);
            int rows = cmd.ExecuteNonQuery();
            return rows > 0;
        }
    }
}