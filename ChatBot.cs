using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Core chatbot logic class. MainWindow.xaml.cs only calls ProcessInput() and GetGreeting().
    /// Routes input through keyword recognition, sentiment detection, memory, conversation flow,
    /// task assistant, quiz game, and activity log — all kept in separate classes (no God class).
    /// </summary>
    public class ChatBot
    {
        private readonly KeywordResponder _keywords;
        private readonly SentimentDetector _sentiment;
        private readonly MemoryStore _memory;
        private readonly DatabaseHelper _db;
        private readonly TaskAssistant _taskAssistant;
        private readonly QuizGame _quiz;
        private readonly ActivityLogger _logger;

        private bool _awaitingName = true;
        private string? _lastTopic = null;

        private readonly List<string> _fallbackResponses = new List<string>
        {
            "I'm not sure I understood that. Try asking about passwords, phishing, privacy, scams, or malware. You can also say 'add a task', 'start quiz', or 'show activity log'.",
            "Hmm, I didn't quite catch that. Type 'help' to see what I can do!",
            "I couldn't find a match for that. Could you try rephrasing, or type 'help' for a list of topics?",
            "That's outside my knowledge for now. Type 'help' to see everything I can assist with."
        };

        private readonly Random _random = new Random();

        public ChatBot()
        {
            _keywords = new KeywordResponder();
            _sentiment = new SentimentDetector();
            _memory = new MemoryStore();
            _db = new DatabaseHelper();
            _logger = new ActivityLogger(_db);
            _taskAssistant = new TaskAssistant(_db, _logger);
            _quiz = new QuizGame();
        }

        public string GetGreeting()
        {
            return "👋 Welcome to the Cybersecurity Awareness Bot!\n\nI'm here to help you stay safe online. Before we start — what's your name?";
        }

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
                return $"Nice to meet you, {_memory.UserName}! 🛡️\n\nI can help with cybersecurity tips, manage tasks, run a quiz, and keep an activity log. Type 'help' anytime to see everything I can do!";
            }

            string lowerInput = trimmed.ToLower();

            // Step 2: Quiz is active — route all input to the quiz first
            if (_quiz.IsActive)
            {
                string quizResponse = _quiz.SubmitAnswer(trimmed);
                if (!_quiz.IsActive)
                    _logger.Log("Quiz completed");
                return quizResponse;
            }

            // Step 3: Awaiting a reminder confirmation from Task Assistant
            if (_taskAssistant.IsAwaitingReminderResponse)
            {
                return _taskAssistant.HandleReminderResponse(trimmed);
            }

            // Step 4: Start quiz command
            if (lowerInput.Contains("start quiz") || lowerInput.Contains("play quiz") || lowerInput.Contains("take quiz") || lowerInput == "quiz")
            {
                _logger.Log("Quiz started");
                return _quiz.StartQuiz();
            }

            // Step 5: Activity log commands
            if (lowerInput.Contains("show activity log") || lowerInput.Contains("activity log") || lowerInput.Contains("what have you done"))
            {
                return _logger.GetFormattedLog();
            }
            if (lowerInput == "show more")
            {
                return _logger.GetFullFormattedLog();
            }

            // Step 6: Task Assistant commands (NLP-style keyword detection)
            if (_taskAssistant.IsViewTasksCommand(lowerInput))
            {
                return _taskAssistant.HandleViewTasks();
            }
            if (_taskAssistant.IsCompleteTaskCommand(lowerInput))
            {
                return _taskAssistant.HandleCompleteTask(trimmed);
            }
            if (_taskAssistant.IsDeleteTaskCommand(lowerInput))
            {
                return _taskAssistant.HandleDeleteTask(trimmed);
            }
            if (_taskAssistant.IsAddTaskCommand(lowerInput))
            {
                return _taskAssistant.HandleAddTask(trimmed);
            }

            // Step 7: Follow-up phrases — continue on the last topic
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

            // Step 8: Interest declarations -> favourite topic
            bool savedTopic = _memory.TryDetectAndStoreFavouriteTopic(lowerInput);
            if (savedTopic)
            {
                string topicConfirm = $"Great! I'll remember that you're interested in **{_memory.FavouriteTopic}** — it's a crucial part of staying safe online. 🧠\n\nHere's a tip to get you started:\n\n";
                string? topicTip = _keywords.GetAnotherResponse(_memory.FavouriteTopic);
                return topicTip != null ? topicConfirm + $"💡 {topicTip}" : topicConfirm + "Keep exploring and asking questions!";
            }

            // Step 9: Help / what can you do
            if (lowerInput == "help" || lowerInput.Contains("what can you do") || lowerInput.Contains("what can i ask"))
            {
                var allKeywords = _keywords.GetAllKeywords();
                string keywordList = string.Join(", ", allKeywords);
                return $"🛡️ Here's everything I can help with:\n\n📋 Cybersecurity topics: {keywordList}\n\n✅ Task Assistant: \"add a task to enable 2FA\", \"show my tasks\", \"complete task 1\", \"delete task 2\"\n\n🎮 Quiz: \"start quiz\"\n\n📜 Activity Log: \"show activity log\"\n\nYou can also say 'tell me more' after any tip!";
            }

            // Step 10: How are you / purpose / about
            if (lowerInput.Contains("how are you") || lowerInput.Contains("how r u"))
                return $"I'm a bot so I don't have feelings, but I'm running perfectly and ready to help you, {_memory.UserName}! 😊";

            if (lowerInput.Contains("purpose") || lowerInput.Contains("what are you"))
                return "My purpose is to educate South African citizens about cybersecurity threats and help you stay organised with cybersecurity tasks. 🛡️";

            if (lowerInput.Contains("your name") || lowerInput.Contains("who are you"))
                return "I'm the Cybersecurity Awareness Bot! I help you learn, stay organised, and test your knowledge.";

            // Step 11: Sentiment detection
            Sentiment detectedSentiment = _sentiment.Detect(lowerInput);
            string sentimentOpener = _sentiment.GetSentimentResponse(detectedSentiment);

            // Step 12: Keyword recognition
            string? matchedKeyword = _keywords.GetMatchedKeyword(lowerInput);
            string? keywordResponse = _keywords.GetResponse(lowerInput);

            if (keywordResponse != null)
            {
                _lastTopic = matchedKeyword;

                string personalisedOpener = _memory.HasFavouriteTopic && _lastTopic == _memory.FavouriteTopic
                    ? _memory.GetPersonalisedOpener()
                    : "";

                string fullResponse = $"{sentimentOpener}{personalisedOpener}💡 {keywordResponse}";
                fullResponse += "\n\n💬 You can say 'tell me more' if you'd like another tip on this topic.";
                return fullResponse;
            }

            // Step 13: Sentiment detected but no keyword
            if (detectedSentiment != Sentiment.Neutral && !string.IsNullOrEmpty(sentimentOpener))
            {
                return $"{sentimentOpener}I can help you with topics like passwords, phishing, privacy, scams, and malware. Which would you like to know more about?";
            }

            // Step 14: Fallback
            string fallback = _fallbackResponses[_random.Next(_fallbackResponses.Count)];
            return $"⚠️ {fallback}";
        }

        public string UserName => _memory.UserName;
    }
}