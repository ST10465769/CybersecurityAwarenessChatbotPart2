using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Core chatbot logic class. MainWindow.xaml.cs only calls ProcessInput() and GetGreeting().
    /// All routing through keyword recognition, sentiment detection, memory, and conversation
    /// flow happens here — keeping MainWindow thin and code well-structured.
    /// </summary>
    public class ChatBot
    {
        private readonly KeywordResponder _keywords;
        private readonly SentimentDetector _sentiment;
        private readonly MemoryStore _memory;

        private bool _awaitingName = true;       // True until user provides their name
        private string? _lastTopic = null;        // Tracks current topic for "tell me more"

        // Fallback responses when nothing else matches
        private readonly List<string> _fallbackResponses = new List<string>
        {
            "I'm not sure I understood that. Try asking about passwords, phishing, privacy, scams, or malware.",
            "Hmm, I didn't quite catch that. Type 'help' to see what topics I can help you with!",
            "I couldn't find a match for that. Could you try rephrasing, or type 'help' for a list of topics?",
            "That's outside my knowledge for now. I can help with cybersecurity topics — type 'help' to see them!"
        };

        private readonly Random _random = new Random();

        public ChatBot()
        {
            _keywords = new KeywordResponder();
            _sentiment = new SentimentDetector();
            _memory = new MemoryStore();
        }

        /// <summary>
        /// Returns the bot's opening message asking for the user's name.
        /// </summary>
        public string GetGreeting()
        {
            return "👋 Welcome to the Cybersecurity Awareness Bot!\n\nI'm here to help you stay safe online. Before we start — what's your name?";
        }

        /// <summary>
        /// Main routing method. Called by MainWindow for every user message.
        /// Returns the bot's complete response string.
        /// </summary>
        public string ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "⚠️ Please type a message. I can't respond to an empty input!";

            string trimmed = input.Trim();

            // Step 1: Capture name on first interaction
            if (_awaitingName)
            {
                _memory.UserName = trimmed;
                _awaitingName = false;
                return $"Nice to meet you, {_memory.UserName}! 🛡️\n\nI'm your Cybersecurity Awareness Assistant. You can ask me about:\npasswords, phishing, privacy, scams, malware, safe browsing, two-factor authentication, or social engineering.\n\nType 'help' anytime to see all topics, or just start chatting!";
            }

            string lowerInput = trimmed.ToLower();

            // Step 2: Handle follow-up phrases — continue on the last topic
            string[] followUps = { "tell me more", "explain more", "give me another tip", "more info", "go on", "continue", "another tip", "more please" };
            foreach (string phrase in followUps)
            {
                if (lowerInput.Contains(phrase))
                {
                    if (_lastTopic != null)
                    {
                        string? followUpResponse = _keywords.GetAnotherResponse(_lastTopic);
                        if (followUpResponse != null)
                        {
                            string opener = _memory.GetPersonalisedOpener();
                            return $"{opener}Here's another tip on {_lastTopic}:\n\n💡 {followUpResponse}";
                        }
                    }
                    return "I don't have a current topic to continue on. Try asking about a specific cybersecurity topic first!";
                }
            }

            // Step 3: Check for interest declarations and store as favourite topic
            bool savedTopic = _memory.TryDetectAndStoreFavouriteTopic(lowerInput);
            if (savedTopic)
            {
                string topicConfirm = $"Great! I'll remember that you're interested in **{_memory.FavouriteTopic}** — it's a crucial part of staying safe online. 🧠\n\nHere's a tip to get you started:\n\n";
                string? topicTip = _keywords.GetAnotherResponse(_memory.FavouriteTopic);
                return topicTip != null ? topicConfirm + $"💡 {topicTip}" : topicConfirm + "Keep exploring and asking questions!";
            }

            // Step 4: Special command — "help" or "what can you do"
            if (lowerInput == "help" || lowerInput.Contains("what can you do") || lowerInput.Contains("what can i ask"))
            {
                var allKeywords = _keywords.GetAllKeywords();
                string keywordList = string.Join(", ", allKeywords);
                return $"🛡️ Here are the cybersecurity topics I can help you with:\n\n📋 {keywordList}\n\nJust mention any of these in your message and I'll give you a helpful tip! You can also say 'tell me more' after any response.";
            }

            // Step 5: "How are you" / purpose / about queries
            if (lowerInput.Contains("how are you") || lowerInput.Contains("how r u"))
                return $"I'm a bot so I don't have feelings, but I'm running perfectly and ready to help you, {_memory.UserName}! 😊 What cybersecurity topic would you like to explore?";

            if (lowerInput.Contains("purpose") || lowerInput.Contains("what are you"))
                return "My purpose is to educate South African citizens about cybersecurity threats and best practices — helping you stay safe from scams, phishing, malware, and more. 🛡️";

            if (lowerInput.Contains("your name") || lowerInput.Contains("who are you"))
                return "I'm the Cybersecurity Awareness Bot! I was created to help people like you stay protected online. What would you like to learn about?";

            // Step 6: Run sentiment detection
            Sentiment detectedSentiment = _sentiment.Detect(lowerInput);
            string sentimentOpener = _sentiment.GetSentimentResponse(detectedSentiment);

            // Step 7: Run keyword recognition
            string? matchedKeyword = _keywords.GetMatchedKeyword(lowerInput);
            string? keywordResponse = _keywords.GetResponse(lowerInput);

            if (keywordResponse != null)
            {
                _lastTopic = matchedKeyword; // Remember for follow-ups

                string personalisedOpener = _memory.HasFavouriteTopic && _lastTopic == _memory.FavouriteTopic
                    ? _memory.GetPersonalisedOpener()
                    : "";

                string fullResponse = $"{sentimentOpener}{personalisedOpener}💡 {keywordResponse}";
                fullResponse += "\n\n💬 You can say 'tell me more' if you'd like another tip on this topic.";
                return fullResponse;
            }

            // Step 8: Sentiment detected but no keyword — still respond empathetically
            if (detectedSentiment != Sentiment.Neutral && !string.IsNullOrEmpty(sentimentOpener))
            {
                return $"{sentimentOpener}I can help you with topics like passwords, phishing, privacy, scams, and malware. Which would you like to know more about?";
            }

            // Step 9: Fallback — unrecognised input
            string fallback = _fallbackResponses[_random.Next(_fallbackResponses.Count)];
            return $"⚠️ {fallback}";
        }

        /// <summary>
        /// Returns the current user's name (used to personalise the GUI header).
        /// </summary>
        public string UserName => _memory.UserName;
    }
}
