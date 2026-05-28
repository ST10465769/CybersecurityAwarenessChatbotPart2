using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Handles keyword recognition and returns randomly selected responses
    /// for cybersecurity topics. Uses a dictionary of keyword → response lists
    /// to satisfy both the Keyword Recognition and Random Responses requirements.
    /// </summary>
    public class KeywordResponder
    {
        private readonly Dictionary<string, List<string>> _responses;
        private readonly Random _random = new Random();

        public KeywordResponder()
        {
            _responses = new Dictionary<string, List<string>>
            {
                ["password"] = new List<string>
                {
                    "Use a strong password with at least 12 characters, mixing uppercase, lowercase, numbers, and symbols. Never reuse passwords across sites!",
                    "A good password is like a toothbrush — use it regularly, make it strong, and never share it with anyone.",
                    "Consider using a reputable password manager to generate and store complex, unique passwords for every account.",
                    "Enable two-factor authentication alongside a strong password for an extra layer of protection.",
                    "Avoid using personal information like birthdays or names in your passwords — hackers check those first."
                },
                ["phishing"] = new List<string>
                {
                    "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations like banks or SARS.",
                    "Check the sender's actual email address carefully — scammers use addresses like 'support@amaz0n.com' to trick you.",
                    "Never click links in unexpected emails. Instead, go directly to the website by typing the address in your browser.",
                    "Phishing messages often create urgency — phrases like 'Your account will be closed in 24 hours' are red flags.",
                    "When in doubt, call the company directly using a number from their official website, not from the email."
                },
                ["privacy"] = new List<string>
                {
                    "Review your social media privacy settings regularly. Limit who can see your personal information and posts.",
                    "Be mindful of what personal data you share online — once it's out there, it's very hard to take back.",
                    "Use a VPN when connecting to public Wi-Fi networks to protect your browsing activity from eavesdroppers.",
                    "Read privacy policies before signing up for new services — look for how your data is stored and shared.",
                    "Regularly audit the apps on your phone and revoke permissions (camera, location, contacts) that apps don't truly need."
                },
                ["scam"] = new List<string>
                {
                    "If an offer seems too good to be true — a lottery win, a big prize, a free phone — it almost certainly is a scam.",
                    "South African phone scams often impersonate SARS, banks, or government agencies. Never give OTP codes over the phone.",
                    "Scammers use urgency and fear to pressure victims. Take a breath, slow down, and verify before you act.",
                    "Report scams to the South African Fraud Prevention Service (SAFPS) at www.safps.org.za.",
                    "Never transfer money or buy gift cards for someone you've only met online, regardless of their story."
                },
                ["malware"] = new List<string>
                {
                    "Keep your operating system and antivirus software updated — most malware exploits outdated software vulnerabilities.",
                    "Only download software from official, trusted sources. Avoid cracked or pirated software, which often contains hidden malware.",
                    "Malware can arrive via email attachments, dodgy downloads, or even infected USB drives. Always scan files before opening.",
                    "Run a reputable antivirus scan regularly. Free options like Windows Defender are decent baselines of protection.",
                    "Back up your important files regularly to an external drive or cloud — ransomware (a type of malware) can hold them hostage."
                },
                ["safe browsing"] = new List<string>
                {
                    "Always check for HTTPS and the padlock icon in your browser before entering any personal or payment information.",
                    "Keep your browser and its extensions updated. Outdated plugins are a common entry point for attackers.",
                    "Be wary of pop-up warnings claiming your computer is infected — these are often scareware trying to get you to install malware.",
                    "Use a browser extension like uBlock Origin to block malicious ads and tracking scripts.",
                    "Avoid logging into sensitive accounts (banking, email) on public or shared computers."
                },
                ["two-factor"] = new List<string>
                {
                    "Two-factor authentication (2FA) adds a second step — usually a code sent to your phone — making it much harder for attackers to access your account.",
                    "Enable 2FA on all important accounts: email, banking, and social media. Even if your password is stolen, 2FA can stop the attacker.",
                    "Use an authenticator app like Google Authenticator or Microsoft Authenticator instead of SMS codes when possible — it's more secure.",
                    "Never share your 2FA codes with anyone, even if they claim to be from your bank or a tech support team.",
                    "Store backup 2FA recovery codes securely offline in case you lose access to your phone."
                },
                ["social engineering"] = new List<string>
                {
                    "Social engineering manipulates people, not technology. Attackers build trust before asking for sensitive information.",
                    "Verify the identity of anyone requesting access to systems or sensitive data — even if they claim to be IT support.",
                    "Be cautious about sharing details on social media; attackers use public info to craft convincing targeted attacks (spear phishing).",
                    "Educate your family and colleagues about social engineering tactics — humans are often the weakest link in cybersecurity.",
                    "Legitimate companies will never ask for your password, PIN, or OTP via phone, email, or chat."
                }
            };
        }

        /// <summary>
        /// Checks if the input contains any known keyword and returns a randomly
        /// selected response from that keyword's list. Returns null if no match found.
        /// </summary>
        public string? GetResponse(string input)
        {
            string lower = input.ToLower();

            foreach (var entry in _responses)
            {
                if (lower.Contains(entry.Key))
                {
                    // Randomly select from the response list for variety
                    int index = _random.Next(entry.Value.Count);
                    return entry.Value[index];
                }
            }

            return null; // No keyword matched
        }

        /// <summary>
        /// Gets a different response for the same keyword (used for "tell me more").
        /// </summary>
        public string? GetAnotherResponse(string keyword)
        {
            if (_responses.TryGetValue(keyword.ToLower(), out List<string>? list))
            {
                int index = _random.Next(list.Count);
                return list[index];
            }
            return null;
        }

        /// <summary>
        /// Returns the matched keyword from user input (used to track _lastTopic).
        /// </summary>
        public string? GetMatchedKeyword(string input)
        {
            string lower = input.ToLower();
            foreach (var key in _responses.Keys)
            {
                if (lower.Contains(key))
                    return key;
            }
            return null;
        }

        /// <summary>
        /// Returns all recognised keywords — used to answer "what can I ask about?".
        /// </summary>
        public List<string> GetAllKeywords()
        {
            return new List<string>(_responses.Keys);
        }
    }
}
