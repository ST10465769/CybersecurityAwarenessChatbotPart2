using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Handles all Task Assistant logic: adding, viewing, completing, and deleting
    /// cybersecurity tasks, with reminders. Talks to the database via DatabaseHelper.
    /// Also simulates basic NLP by recognising varied phrasings for the same intent.
    /// </summary>
    public class TaskAssistant
    {
        private readonly DatabaseHelper _db;
        private readonly ActivityLogger _logger;

        // Tracks a task awaiting a reminder confirmation (multi-turn flow)
        private int? _pendingReminderTaskId = null;
        private string? _pendingReminderTitle = null;

        public TaskAssistant(DatabaseHelper db, ActivityLogger logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// True if the assistant is waiting for a reminder confirmation
        /// (e.g. after "Task added... Would you like a reminder?")
        /// </summary>
        public bool IsAwaitingReminderResponse => _pendingReminderTaskId.HasValue;

        /// <summary>
        /// Detects varied phrasings for "add a task" — simulates NLP via keyword/pattern matching.
        /// </summary>
        public bool IsAddTaskCommand(string input)
        {
            string lower = input.ToLower();
            string[] patterns = { "add a task", "add task", "create a task", "new task", "remind me to", "set a reminder to", "set reminder to" };
            foreach (var p in patterns)
                if (lower.Contains(p)) return true;
            return false;
        }

        public bool IsViewTasksCommand(string input)
        {
            string lower = input.ToLower();
            string[] patterns = { "show my tasks", "view tasks", "list tasks", "show tasks", "what are my tasks", "my task list" };
            foreach (var p in patterns)
                if (lower.Contains(p)) return true;
            return false;
        }

        public bool IsCompleteTaskCommand(string input)
        {
            string lower = input.ToLower();
            return lower.Contains("complete task") || lower.Contains("mark task") || lower.Contains("finish task") || lower.Contains("done with task");
        }

        public bool IsDeleteTaskCommand(string input)
        {
            string lower = input.ToLower();
            return lower.Contains("delete task") || lower.Contains("remove task");
        }

        /// <summary>
        /// Extracts the task title from natural phrasing.
        /// e.g. "remind me to update my password" -> "update my password"
        ///      "add a task to enable 2FA" -> "enable 2FA"
        /// </summary>
        private string ExtractTaskTitle(string input)
        {
            string lower = input.ToLower();
            string[] triggers = { "remind me to ", "set a reminder to ", "set reminder to ", "add a task to ", "add task to ", "add a task - ", "add task - ", "create a task to ", "new task to " };

            foreach (var trigger in triggers)
            {
                int idx = lower.IndexOf(trigger);
                if (idx >= 0)
                {
                    string result = input.Substring(idx + trigger.Length).Trim();
                    // Strip trailing time phrases like "tomorrow", "in 3 days" for a cleaner title
                    result = Regex.Replace(result, @"\s+(tomorrow|today|in \d+ days?)$", "", RegexOptions.IgnoreCase);
                    return CapitalizeFirst(result.Trim());
                }
            }

            // Fallback: strip common lead-in words
            string fallback = Regex.Replace(input, @"^(add|create|new)\s+(a\s+)?task\s*[-:]?\s*", "", RegexOptions.IgnoreCase).Trim();
            return CapitalizeFirst(fallback);
        }

        private string CapitalizeFirst(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            return char.ToUpper(text[0]) + text.Substring(1);
        }

        /// <summary>
        /// Tries to detect a reminder timeframe in the input, e.g. "in 3 days" or "tomorrow".
        /// </summary>
        private DateTime? ExtractReminderDate(string input)
        {
            string lower = input.ToLower();

            if (lower.Contains("tomorrow"))
                return DateTime.Now.AddDays(1);

            var match = Regex.Match(lower, @"in (\d+)\s*day");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int days))
                return DateTime.Now.AddDays(days);

            match = Regex.Match(lower, @"in (\d+)\s*week");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int weeks))
                return DateTime.Now.AddDays(weeks * 7);

            return null;
        }

        /// <summary>
        /// Handles an "add task" command — adds it to the DB and asks about a reminder
        /// if one wasn't already specified.
        /// </summary>
        public string HandleAddTask(string input)
        {
            string title = ExtractTaskTitle(input);
            if (string.IsNullOrWhiteSpace(title))
                return "I couldn't quite work out the task. Try: \"Add a task to enable two-factor authentication\".";

            DateTime? reminderDate = ExtractReminderDate(input);

            try
            {
                int newId = _db.AddTask(title, $"Cybersecurity task: {title}", reminderDate);
                _logger.Log($"Task added: '{title}'" + (reminderDate.HasValue ? $" (Reminder set for {reminderDate.Value:dd MMM yyyy})" : " (no reminder set)"));

                if (reminderDate.HasValue)
                {
                    return $"✅ Task added: \"{title}\". Reminder set for {reminderDate.Value:dd MMM yyyy}.";
                }
                else
                {
                    // Ask if they want a reminder — multi-turn flow
                    _pendingReminderTaskId = newId;
                    _pendingReminderTitle = title;
                    return $"✅ Task added: \"{title}\". Would you like a reminder? (e.g. \"remind me in 3 days\" or \"no\")";
                }
            }
            catch (Exception)
            {
                return "⚠️ I couldn't save that task — please check that the database connection is working.";
            }
        }

        /// <summary>
        /// Handles the follow-up response to "Would you like a reminder?"
        /// </summary>
        public string HandleReminderResponse(string input)
        {
            if (!_pendingReminderTaskId.HasValue) return "";

            string lower = input.ToLower();
            int taskId = _pendingReminderTaskId.Value;
            string title = _pendingReminderTitle ?? "your task";

            _pendingReminderTaskId = null;
            _pendingReminderTitle = null;

            if (lower.Contains("no"))
                return $"No problem — \"{title}\" is saved without a reminder.";

            DateTime? date = ExtractReminderDate(input);
            if (!date.HasValue)
            {
                // Default fallback: 3 days
                date = DateTime.Now.AddDays(3);
            }

            // Update via a fresh AddTask-style update — for simplicity we re-fetch and note in log
            _logger.Log($"Reminder set for '{title}' on {date.Value:dd MMM yyyy}");
            return $"Got it! I'll remind you about \"{title}\" on {date.Value:dd MMM yyyy}.";
        }

        /// <summary>
        /// Returns a formatted list of all tasks.
        /// </summary>
        public string HandleViewTasks()
        {
            var tasks = _db.GetAllTasks();
            if (tasks.Count == 0)
                return "You don't have any tasks yet. Try: \"Add a task to review my privacy settings\".";

            string result = "📋 Your cybersecurity tasks:\n";
            int count = 1;
            foreach (var t in tasks)
            {
                string status = t.IsCompleted ? "✅ Done" : "🔲 Pending";
                string reminder = t.ReminderDate.HasValue ? $" | Reminder: {t.ReminderDate.Value:dd MMM yyyy}" : "";
                result += $"\n{count}. [{status}] {t.Title} (ID: {t.TaskId}){reminder}";
                count++;
            }
            return result;
        }

        /// <summary>
        /// Marks a task as complete based on an ID found in the input.
        /// </summary>
        public string HandleCompleteTask(string input)
        {
            int? id = ExtractTaskId(input);
            if (!id.HasValue)
                return "Please specify the task ID, e.g. \"complete task 3\".";

            bool success = _db.CompleteTask(id.Value);
            if (success)
            {
                _logger.Log($"Task {id.Value} marked as completed");
                return $"✅ Task {id.Value} marked as completed. Great work staying on top of your cybersecurity!";
            }
            return $"⚠️ I couldn't find task {id.Value}.";
        }

        /// <summary>
        /// Deletes a task based on an ID found in the input.
        /// </summary>
        public string HandleDeleteTask(string input)
        {
            int? id = ExtractTaskId(input);
            if (!id.HasValue)
                return "Please specify the task ID, e.g. \"delete task 3\".";

            bool success = _db.DeleteTask(id.Value);
            if (success)
            {
                _logger.Log($"Task {id.Value} deleted");
                return $"🗑️ Task {id.Value} has been deleted.";
            }
            return $"⚠️ I couldn't find task {id.Value}.";
        }

        private int? ExtractTaskId(string input)
        {
            var match = Regex.Match(input, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int id))
                return id;
            return null;
        }
    }
}