using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Detects the emotional tone/sentiment in user input and returns
    /// an empathetic opening phrase before the chatbot gives its tip.
    /// </summary>
    public enum Sentiment
    {
        Neutral,
        Worried,
        Curious,
        Frustrated,
        Happy
    }

    public class SentimentDetector
    {
        // Maps each sentiment to its list of trigger words
        private readonly Dictionary<Sentiment, List<string>> _triggerWords;

        // Maps each sentiment to an empathetic response opener
        private readonly Dictionary<Sentiment, string> _sentimentResponses;

        public SentimentDetector()
        {
            _triggerWords = new Dictionary<Sentiment, List<string>>
            {
                [Sentiment.Worried] = new List<string>
                {
                    "worried", "scared", "afraid", "anxious", "nervous",
                    "unsafe", "fear", "frightened", "concerned", "uneasy"
                },
                [Sentiment.Curious] = new List<string>
                {
                    "curious", "wondering", "interested", "want to know",
                    "how does", "what is", "tell me about", "explain",
                    "can you tell", "i'd like to know"
                },
                [Sentiment.Frustrated] = new List<string>
                {
                    "frustrated", "annoyed", "confused", "don't understand",
                    "not sure", "complicated", "difficult", "hard to",
                    "makes no sense", "irritated", "fed up"
                },
                [Sentiment.Happy] = new List<string>
                {
                    "great", "thanks", "helpful", "awesome", "love it",
                    "amazing", "excellent", "perfect", "brilliant", "thank you"
                }
            };

            _sentimentResponses = new Dictionary<Sentiment, string>
            {
                [Sentiment.Worried] =
                    "I completely understand — it's natural to feel that way about cybersecurity threats. You're not alone, and being aware is already the first step. Here's what you should know: ",
                [Sentiment.Curious] =
                    "Great curiosity! Wanting to learn is the best way to stay protected online. Here's some useful information: ",
                [Sentiment.Frustrated] =
                    "I hear you — cybersecurity can feel overwhelming at times, but let's break it down simply. Here's a clear explanation: ",
                [Sentiment.Happy] =
                    "Glad to hear that! Let's keep the momentum going — here's another tip to help you stay safe: ",
                [Sentiment.Neutral] = ""  // No opener needed for neutral
            };
        }

        /// <summary>
        /// Analyses the user input and returns the detected Sentiment.
        /// </summary>
        public Sentiment Detect(string input)
        {
            string lower = input.ToLower();

            foreach (var entry in _triggerWords)
            {
                foreach (string trigger in entry.Value)
                {
                    if (lower.Contains(trigger))
                        return entry.Key;
                }
            }

            return Sentiment.Neutral;
        }

        /// <summary>
        /// Returns the empathetic opening phrase for the given sentiment.
        /// Returns an empty string for Neutral so nothing is prepended.
        /// </summary>
        public string GetSentimentResponse(Sentiment sentiment)
        {
            return _sentimentResponses.TryGetValue(sentiment, out string? response)
                ? response
                : "";
        }
    }
}