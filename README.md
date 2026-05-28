# 🛡️ Cybersecurity Awareness Chatbot — Part 2

**Student Name:** Simamkele Kinola Kaulela
**Student Number:** ST10465769
**Module:** PROG6221 — Programming 2A
**Institution:** The Independent Institute of Education (IIE)
**Part:** Part 2 — GUI Interface, Keyword Recognition, Sentiment Detection, and Memory

---

## 📌 Project Description

The Cybersecurity Awareness Chatbot is a WPF (Windows Presentation Foundation) GUI application designed to educate South African citizens about cybersecurity threats and best practices. It simulates a conversational assistant that recognises cybersecurity keywords, detects user sentiment, remembers user details, and responds with relevant, varied tips.

---

## ✅ Features Implemented in Part 2

- **WPF GUI** — Dark-themed, professional chat interface with header, chat display, input box, and send button
- **Voice Greeting** — Plays `greeting.wav` on application launch using `System.Media.SoundPlayer`
- **ASCII Art** — Cybersecurity-themed logo displayed in the GUI header
- **Keyword Recognition** — Recognises 8 cybersecurity keywords with targeted responses:
  - password, phishing, privacy, scam, malware, safe browsing, two-factor, social engineering
- **Random Responses** — Each keyword has 5 responses; one is randomly selected per interaction
- **Conversation Flow** — "tell me more" and similar phrases continue the current topic without resetting
- **Memory and Recall** — Remembers the user's name and favourite cybersecurity topic; uses them to personalise responses
- **Sentiment Detection** — Detects worried, curious, frustrated, and happy sentiments; responds empathetically and automatically provides a tip
- **Input Validation** — Handles empty input and unrecognised queries gracefully with fallback responses
- **Code Optimisation** — Logic split across 4 dedicated classes (no God class)

---

## 🗂️ Project Structure

```
CybersecurityChatbotPart2/
├── App.xaml                  # WPF application entry point
├── App.xaml.cs
├── MainWindow.xaml           # GUI layout — dark cybersecurity theme
├── MainWindow.xaml.cs        # Thin code-behind — UI events only
├── ChatBot.cs                # Core logic — routes all input
├── KeywordResponder.cs       # Keyword dictionary and random responses
├── SentimentDetector.cs      # Sentiment detection and empathetic responses
├── MemoryStore.cs            # Stores user name and favourite topic
├── CybersecurityChatbot.csproj
├── greeting.wav              # Voice greeting audio file
└── .github/
    └── workflows/
        └── build.yml         # GitHub Actions CI workflow
```

---

## 🛠️ Prerequisites

- **Visual Studio 2022** or later
- **.NET 8.0 SDK** (Windows)
- **Windows OS** (required for WPF)

---

## ▶️ How to Clone and Run

### Step 1 — Clone the repository
```
git clone https://github.com/ST10465769/CybersecurityAwarenessChatbotPart2.git
```

### Step 2 — Open in Visual Studio
1. Open Visual Studio 2022
2. Click **"Open a project or solution"**
3. Navigate to the cloned folder and open `CybersecurityChatbot.csproj`

### Step 3 — Ensure greeting.wav is present
- The `greeting.wav` file must be in the project root folder
- In Solution Explorer, right-click it → Properties → set **"Copy to Output Directory"** to **"Copy always"**

### Step 4 — Build and Run
- Press **Ctrl+Shift+B** to build
- Press **F5** to run
- The GUI will launch, play the voice greeting, and prompt you for your name

---

## 💬 How to Use the Chatbot

| What you type | What happens |
|---|---|
| Your name (first message) | Bot greets you personally |
| `tell me about phishing` | Keyword response with cybersecurity tip |
| `I'm worried about scams` | Empathetic opener + automatic tip |
| `I'm interested in privacy` | Bot saves it as your favourite topic |
| `tell me more` | Bot continues on the current topic |
| `help` | Bot lists all 8 available topics |
| Any unrecognised input | Friendly fallback message — no crash |

---

## 🔁 GitHub Actions CI

The project includes a GitHub Actions workflow that automatically builds the project on every push to confirm the code compiles successfully.

**Workflow file:** `.github/workflows/build.yml`
**Build environment:** `windows-latest` (.NET 8, WPF)


---

## 📦 GitHub Releases

| Tag | Description |
|---|---|
| `v2.0` | Initial release — WPF GUI, keyword recognition, random responses, voice greeting |
| `v2.1` | Full release — Added sentiment detection, memory/recall, conversation follow-up flow |

---

## 👨‍💻 Author

**Simamkele Kinola Kaulela**
Student Number: ST10465769
The Independent Institute of Education — 2026
