using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Code-behind for the main window. This file is intentionally thin —
    /// it only handles UI events and delegates all logic to ChatBot.
    /// No chatbot logic lives here (Code Optimisation requirement).
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ChatBot _chatBot;  // The only field needed here

        public MainWindow()
        {
            InitializeComponent();

            // Initialise the ChatBot — all logic is in that class
            _chatBot = new ChatBot();

            // Startup tasks: voice greeting, then show bot's first message
            PlayVoiceGreeting();
            AppendBotMessage(_chatBot.GetGreeting());
        }

        // ─── EVENT HANDLERS ─────────────────────────────────────────────────

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void UserInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Pressing Enter sends the message (no need to click the button)
            if (e.Key == Key.Enter)
                SendMessage();
        }

        // ─── CORE SEND LOGIC ────────────────────────────────────────────────

        private void SendMessage()
        {
            string userInput = UserInputBox.Text.Trim();

            // Input validation — don't send empty messages
            if (string.IsNullOrWhiteSpace(userInput))
            {
                StatusLabel.Text = "⚠️ Please type a message before sending.";
                return;
            }

            // Display user message in the chat
            AppendUserMessage(userInput);

            // Clear input box immediately so it feels responsive
            UserInputBox.Clear();
            UserInputBox.Focus();

            // Get bot response (all logic happens in ChatBot.ProcessInput)
            string response = _chatBot.ProcessInput(userInput);

            // Display bot response
            AppendBotMessage(response);

            // Update status with user name once known
            if (!string.IsNullOrWhiteSpace(_chatBot.UserName))
                StatusLabel.Text = $"🟢 Chatting with {_chatBot.UserName}";

            // Auto-scroll to the latest message
            ScrollToBottom();
        }

        // ─── CHAT DISPLAY HELPERS ────────────────────────────────────────────

        /// <summary>
        /// Adds a user message bubble to the chat panel.
        /// </summary>
        private void AppendUserMessage(string message)
        {
            // Outer right-aligned container
            var outerPanel = new DockPanel { Margin = new Thickness(60, 6, 0, 6) };

            // Bubble
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 180, 216)),  // #00B4D8
                CornerRadius = new CornerRadius(12, 12, 2, 12),
                Padding = new Thickness(12, 8, 12, 8),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth = 520
            };

            var text = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(Color.FromRgb(13, 17, 23)),  // #0D1117
                FontSize = 13,
                FontFamily = new FontFamily("Segoe UI"),
                TextWrapping = TextWrapping.Wrap
            };

            border.Child = text;
            outerPanel.Children.Add(border);
            ChatPanel.Children.Add(outerPanel);
        }

        /// <summary>
        /// Adds a bot message bubble to the chat panel.
        /// </summary>
        private void AppendBotMessage(string message)
        {
            // Label row: bot icon + name
            var labelPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 6, 60, 2)
            };
            labelPanel.Children.Add(new TextBlock
            {
                Text = "🤖 Bot",
                Foreground = new SolidColorBrush(Color.FromRgb(139, 148, 158)),  // #8B949E
                FontSize = 11,
                FontFamily = new FontFamily("Segoe UI"),
                VerticalAlignment = VerticalAlignment.Center
            });

            // Bubble
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(22, 27, 34)),   // #161B22
                BorderBrush = new SolidColorBrush(Color.FromRgb(33, 38, 45)),  // #21262D
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2, 12, 12, 12),
                Padding = new Thickness(14, 10, 14, 10),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 580,
                Margin = new Thickness(0, 0, 60, 6)
            };

            var text = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),  // #E6EDF3
                FontSize = 13,
                FontFamily = new FontFamily("Segoe UI"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };

            border.Child = text;
            ChatPanel.Children.Add(labelPanel);
            ChatPanel.Children.Add(border);
        }

        /// <summary>
        /// Scrolls the chat view to the latest message automatically.
        /// </summary>
        private void ScrollToBottom()
        {
            ChatScrollViewer.UpdateLayout();
            ChatScrollViewer.ScrollToBottom();
        }

        // ─── VOICE GREETING ──────────────────────────────────────────────────

        /// <summary>
        /// Plays the greeting.wav voice file on startup.
        /// Silently skips if the file is not found (so the app still runs).
        /// </summary>
        private void PlayVoiceGreeting()
        {
            try
            {
                // Look for greeting.wav next to the executable
                string wavPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");

                if (File.Exists(wavPath))
                {
                    SoundPlayer player = new SoundPlayer(wavPath);
                    player.Play();  // Async play — doesn't block the UI
                }
                else
                {
                    // File not found is not a crash — just update the status
                    StatusLabel.Text = "ℹ️ greeting.wav not found — place it in the project folder and set 'Copy Always'";
                }
            }
            catch (Exception ex)
            {
                // Log to status bar; never crash the app over a missing sound
                StatusLabel.Text = $"ℹ️ Could not play greeting: {ex.Message}";
            }
        }
    }
}