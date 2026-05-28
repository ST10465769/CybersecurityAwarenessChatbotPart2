using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Stores and recalls information about the user during a conversation.
    /// Remembers name, favourite topic, and any other key-value pairs.
    /// Used to personalise chatbot responses throughout the session.
    /// </summary>
    public class MemoryStore
    {
        // Auto-properties for the two required memory items
        public string UserName { get; set; } = "";
        public string FavouriteTopic { get; set; } = "";

        // General-purpose key-value store for any extra recalled data
        private readonly Dictionary<string, string> _memory = new Dictionary<string, string>();

        /// <summary>
        /// Saves any key-value pair to memory.
        /// </summary>
        public void Store(string key, string value)
        {
            _memory[key.ToLower()] = value;
        }

        /// <summary>
        /// Retrieves a previously stored value by key. Returns null if not found.
        /// </summary>
        public string? Recall(string key)
        {
            return _memory.TryGetValue(key.ToLower(), out string? value) ? value : null;
        }

        /// <summary>
        /// Checks whether the user has shared a favourite topic yet.
        /// </summary>
        public bool HasFavouriteTopic => !string.IsNullOrWhiteSpace(FavouriteTopic);

        /// <summary>
        /// Builds a personalised opener for bot responses using stored memory.
        /// Returns empty string if no personalisation data is available yet.
        /// </summary>
        public string GetPersonalisedOpener()
        {
            if (!string.IsNullOrWhiteSpace(FavouriteTopic) && !string.IsNullOrWhiteSpace(UserName))
                return $"As someone interested in {FavouriteTopic}, {UserName}, here's something relevant for you — ";

            if (!string.IsNullOrWhiteSpace(FavouriteTopic))
                return $"As someone interested in {FavouriteTopic}, here's something relevant — ";

            if (!string.IsNullOrWhiteSpace(UserName))
                return $"Good to hear from you, {UserName}! ";

            return "";
        }

        /// <summary>
        /// Checks whether user input expresses interest in a topic
        /// and stores it as the favourite topic if so.
        /// Returns true if a new topic was saved.
        /// </summary>
        public bool TryDetectAndStoreFavouriteTopic(string input)
        {
            string lower = input.ToLower();

            string[] interestPhrases = { "interested in", "i like", "i love", "my favourite", "i want to learn about", "tell me more about" };
            string[] topics = { "password", "phishing", "privacy", "scam", "malware", "safe browsing", "two-factor", "social engineering" };

            foreach (string phrase in interestPhrases)
            {
                if (lower.Contains(phrase))
                {
                    foreach (string topic in topics)
                    {
                        if (lower.Contains(topic))
                        {
                            FavouriteTopic = topic;
                            Store("favouriteTopic", topic);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}