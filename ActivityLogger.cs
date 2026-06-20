using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Records chatbot actions (tasks, reminders, quiz activity, NLP interactions)
    /// to the ActivityLog table via DatabaseHelper, and formats them for display.
    /// </summary>
    public class ActivityLogger
    {
        private readonly DatabaseHelper _db;

        public ActivityLogger(DatabaseHelper db)
        {
            _db = db;
        }

        /// <summary>
        /// Adds an entry to the activity log.
        /// </summary>
        public void Log(string description)
        {
            try
            {
                _db.AddLogEntry(description);
            }
            catch
            {
                // Don't let logging failures crash the chatbot
            }
        }

        /// <summary>
        /// Returns the most recent log entries (default: last 5), formatted for chat display.
        /// </summary>
        public string GetFormattedLog(int maxCount = 5)
        {
            List<string> entries;
            try
            {
                entries = _db.GetRecentLogEntries(maxCount);
            }
            catch
            {
                return "⚠️ I couldn't retrieve the activity log right now.";
            }

            if (entries.Count == 0)
                return "No activity has been logged yet.";

            string result = $"📜 Here's a summary of your last {entries.Count} action(s):\n";
            int count = 1;
            foreach (var entry in entries)
            {
                result += $"\n{count}. {entry}";
                count++;
            }

            result += "\n\nType \"show more\" to see additional history.";
            return result;
        }

        /// <summary>
        /// Returns a longer history (used for "show more").
        /// </summary>
        public string GetFullFormattedLog()
        {
            return GetFormattedLog(50);
        }
    }
}