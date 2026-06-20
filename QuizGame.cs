using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Represents a single quiz question — supports both multiple-choice and true/false.
    /// </summary>
    public class QuizQuestion
    {
        public string Question { get; set; } = "";
        public List<string> Options { get; set; } = new();
        public int CorrectOptionIndex { get; set; }
        public string Explanation { get; set; } = "";
    }

    /// <summary>
    /// Manages the cybersecurity mini-game: question bank, current progress,
    /// scoring, and feedback. ChatBot.cs delegates all quiz logic to this class.
    /// </summary>
    public class QuizGame
    {
        private readonly List<QuizQuestion> _questions;
        private int _currentQuestionIndex = -1;
        private int _score = 0;
        private bool _quizActive = false;

        public bool IsActive => _quizActive;

        public QuizGame()
        {
            _questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "What should you do if you receive an email asking for your password?",
                    Options = new List<string> { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                    CorrectOptionIndex = 2,
                    Explanation = "Reporting phishing emails helps prevent scams and protects others too."
                },
                new QuizQuestion
                {
                    Question = "True or False: It's safe to use the same password across multiple accounts.",
                    Options = new List<string> { "True", "False" },
                    CorrectOptionIndex = 1,
                    Explanation = "False — if one account is breached, all accounts using that password become vulnerable."
                },
                new QuizQuestion
                {
                    Question = "Which of these is a sign of a phishing email?",
                    Options = new List<string> { "Urgent language demanding immediate action", "Personalised greeting with your full name", "Coming from a known contact", "Professional company logo" },
                    CorrectOptionIndex = 0,
                    Explanation = "Urgency is a classic phishing tactic designed to make you act before you think."
                },
                new QuizQuestion
                {
                    Question = "True or False: Public Wi-Fi networks are always safe for online banking.",
                    Options = new List<string> { "True", "False" },
                    CorrectOptionIndex = 1,
                    Explanation = "False — public Wi-Fi can be intercepted; avoid sensitive transactions without a VPN."
                },
                new QuizQuestion
                {
                    Question = "What does 2FA stand for?",
                    Options = new List<string> { "Two-Factor Authentication", "Two-File Access", "Total Fraud Avoidance", "Two-Factor Access" },
                    CorrectOptionIndex = 0,
                    Explanation = "Two-Factor Authentication adds a second verification step beyond your password."
                },
                new QuizQuestion
                {
                    Question = "True or False: Updating your software regularly helps protect against malware.",
                    Options = new List<string> { "True", "False" },
                    CorrectOptionIndex = 0,
                    Explanation = "True — updates often patch security vulnerabilities that malware exploits."
                },
                new QuizQuestion
                {
                    Question = "Which password is the strongest?",
                    Options = new List<string> { "password123", "MyDog2020", "Tr0ub4dor&9$kZw!", "yourname1" },
                    CorrectOptionIndex = 2,
                    Explanation = "A long mix of upper/lowercase, numbers, and symbols is much harder to crack."
                },
                new QuizQuestion
                {
                    Question = "What is 'social engineering' in cybersecurity?",
                    Options = new List<string> { "Building secure networks", "Manipulating people into revealing information", "A type of antivirus", "Encrypting social media" },
                    CorrectOptionIndex = 1,
                    Explanation = "Social engineering exploits human trust rather than technical vulnerabilities."
                },
                new QuizQuestion
                {
                    Question = "True or False: You should click links in emails from unknown senders to verify they're legitimate.",
                    Options = new List<string> { "True", "False" },
                    CorrectOptionIndex = 1,
                    Explanation = "False — never click unknown links; verify through official channels instead."
                },
                new QuizQuestion
                {
                    Question = "What is the safest way to check if a website is secure before entering payment details?",
                    Options = new List<string> { "Check for HTTPS and a padlock icon", "Check if the page loads quickly", "Check the background colour", "Check the number of ads" },
                    CorrectOptionIndex = 0,
                    Explanation = "HTTPS and a padlock indicate an encrypted connection, though always stay cautious."
                },
                new QuizQuestion
                {
                    Question = "True or False: Malware can only infect your computer through email attachments.",
                    Options = new List<string> { "True", "False" },
                    CorrectOptionIndex = 1,
                    Explanation = "False — malware can spread via downloads, USB drives, infected websites, and more."
                },
                new QuizQuestion
                {
                    Question = "What should you do immediately if you suspect you've fallen for a scam?",
                    Options = new List<string> { "Ignore it and hope nothing happens", "Change your passwords and contact your bank", "Wait a few weeks to see what happens", "Tell no one" },
                    CorrectOptionIndex = 1,
                    Explanation = "Acting quickly — changing passwords and alerting your bank — limits potential damage."
                }
            };
        }

        /// <summary>
        /// Starts a new quiz attempt, resetting score and progress.
        /// </summary>
        public string StartQuiz()
        {
            _currentQuestionIndex = 0;
            _score = 0;
            _quizActive = true;
            return GetCurrentQuestionText();
        }

        /// <summary>
        /// Returns the formatted text for the current question.
        /// </summary>
        public string GetCurrentQuestionText()
        {
            if (_currentQuestionIndex < 0 || _currentQuestionIndex >= _questions.Count)
                return "";

            var q = _questions[_currentQuestionIndex];
            string optionsText = "";
            for (int i = 0; i < q.Options.Count; i++)
            {
                optionsText += $"\n{(char)('A' + i)}) {q.Options[i]}";
            }

            return $"❓ Question {_currentQuestionIndex + 1}/{_questions.Count}:\n{q.Question}{optionsText}\n\nType the letter of your answer (e.g. A).";
        }

        /// <summary>
        /// Processes the user's answer, gives feedback, and advances to the next question.
        /// Returns the response text including feedback and the next question (or final score).
        /// </summary>
        public string SubmitAnswer(string userAnswer)
        {
            if (!_quizActive || _currentQuestionIndex >= _questions.Count)
                return "There's no active quiz question right now. Type 'start quiz' to begin!";

            var q = _questions[_currentQuestionIndex];
            string trimmed = userAnswer.Trim().ToUpper();

            int selectedIndex = -1;
            if (trimmed.Length > 0 && trimmed[0] >= 'A' && trimmed[0] <= 'Z')
                selectedIndex = trimmed[0] - 'A';

            string feedback;
            if (selectedIndex == q.CorrectOptionIndex)
            {
                _score++;
                feedback = $"✅ Correct! {q.Explanation}";
            }
            else if (selectedIndex >= 0 && selectedIndex < q.Options.Count)
            {
                feedback = $"❌ Not quite. The correct answer was {(char)('A' + q.CorrectOptionIndex)}) {q.Options[q.CorrectOptionIndex]}. {q.Explanation}";
            }
            else
            {
                feedback = $"⚠️ I didn't recognise that option. The correct answer was {(char)('A' + q.CorrectOptionIndex)}) {q.Options[q.CorrectOptionIndex]}. {q.Explanation}";
            }

            _currentQuestionIndex++;

            if (_currentQuestionIndex >= _questions.Count)
            {
                _quizActive = false;
                return $"{feedback}\n\n🏁 Quiz complete! You scored {_score}/{_questions.Count}.\n{GetScoreFeedback()}";
            }

            return $"{feedback}\n\n{GetCurrentQuestionText()}";
        }

        private string GetScoreFeedback()
        {
            double percentage = (double)_score / _questions.Count * 100;
            if (percentage >= 80)
                return "🎉 Great job! You're a cybersecurity pro!";
            if (percentage >= 50)
                return "👍 Good effort! Keep learning to sharpen your skills.";
            return "📚 Keep learning to stay safe online — review the topics and try again!";
        }
    }
}
