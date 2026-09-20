using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HangmanTwist
{
    public class MainForm : Form
    {
        // Word pools by difficulty
        private readonly string[] easyWords = { "cat", "dog", "sun", "book", "tree", "fish", "ball", "milk" };
        private readonly string[] mediumWords = { "castle", "guitar", "planet", "bridge", "dragon", "puzzle", "forest" };
        private readonly string[] hardWords = { "xylophone", "chrysalis", "labyrinth", "quixotic", "bureaucracy", "zeitgeist" };

        private const int MaxWrongGuesses = 6;

        private readonly Random rand = new Random();
        private HashSet<char> guessedLetters = new HashSet<char>();
        private string currentWord = "";
        private string currentCategory = "Easy";
        private int wrongGuesses = 0;
        private bool gameOver = false;

        // UI controls
        private Label lblCategory;
        private Label lblHangmanArt;
        private Label lblWordDisplay;
        private Label lblWrongLetters;
        private Label lblRemaining;
        private Label lblStatus;
        private FlowLayoutPanel letterPanel;
        private Button btnNewGame;
        private Dictionary<char, Button> letterButtons = new Dictionary<char, Button>();

        public MainForm()
        {
            InitializeUI();
            StartNewGame();
        }

        private void InitializeUI()
        {
            this.Text = "Hangman with a Twist";
            this.Width = 640;
            this.Height = 560;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            lblCategory = new Label
            {
                Text = "Category: Easy",
                Font = new Font("Consolas", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 15)
            };

            lblHangmanArt = new Label
            {
                Text = GetHangmanArt(0),
                Font = new Font("Consolas", 14),
                AutoSize = true,
                Location = new Point(20, 45)
            };

            lblWordDisplay = new Label
            {
                Text = "",
                Font = new Font("Consolas", 20, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 210)
            };

            lblWrongLetters = new Label
            {
                Text = "Wrong guesses: none",
                Font = new Font("Consolas", 11),
                AutoSize = true,
                Location = new Point(20, 250)
            };

            lblRemaining = new Label
            {
                Text = "Remaining tries: " + MaxWrongGuesses,
                Font = new Font("Consolas", 11),
                AutoSize = true,
                Location = new Point(20, 275)
            };

            lblStatus = new Label
            {
                Text = "",
                Font = new Font("Consolas", 12, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.DarkRed,
                Location = new Point(20, 305)
            };

            letterPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 340),
                Size = new Size(580, 140),
                AutoScroll = true
            };

            btnNewGame = new Button
            {
                Text = "New Game",
                Font = new Font("Consolas", 11),
                Location = new Point(20, 490),
                Size = new Size(120, 35)
            };
            btnNewGame.Click += (s, e) => StartNewGame();

            // Create A-Z letter buttons
            for (char c = 'A'; c <= 'Z'; c++)
            {
                char letter = c;
                Button btn = new Button
                {
                    Text = letter.ToString(),
                    Font = new Font("Consolas", 10, FontStyle.Bold),
                    Size = new Size(38, 38),
                    Margin = new Padding(3)
                };
                btn.Click += (s, e) => OnLetterGuessed(letter);
                letterButtons[letter] = btn;
                letterPanel.Controls.Add(btn);
            }

            this.Controls.Add(lblCategory);
            this.Controls.Add(lblHangmanArt);
            this.Controls.Add(lblWordDisplay);
            this.Controls.Add(lblWrongLetters);
            this.Controls.Add(lblRemaining);
            this.Controls.Add(lblStatus);
            this.Controls.Add(letterPanel);
            this.Controls.Add(btnNewGame);
        }

        private void StartNewGame()
        {
            wrongGuesses = 0;
            gameOver = false;
            guessedLetters.Clear();
            currentCategory = "Easy";
            currentWord = GetRandomWord(easyWords);

            lblStatus.Text = "";

            foreach (var btn in letterButtons.Values)
            {
                btn.Enabled = true;
            }

            RefreshDisplay();
        }

        private void OnLetterGuessed(char letter)
        {
            if (gameOver) return;

            char lowerLetter = char.ToLower(letter);
            letterButtons[letter].Enabled = false;

            if (guessedLetters.Contains(lowerLetter))
            {
                return; // already guessed, shouldn't happen since button gets disabled
            }

            guessedLetters.Add(lowerLetter);

            if (currentWord.Contains(lowerLetter))
            {
                // Correct guess
                if (currentWord.All(c => guessedLetters.Contains(c)))
                {
                    gameOver = true;
                    lblStatus.ForeColor = Color.DarkGreen;
                    lblStatus.Text = "You win! The word was: " + currentWord.ToUpper();
                    DisableAllLetterButtons();
                }
            }
            else
            {
                // Wrong guess
                wrongGuesses++;

                if (wrongGuesses >= MaxWrongGuesses)
                {
                    gameOver = true;
                    lblStatus.ForeColor = Color.DarkRed;
                    lblStatus.Text = "Game over! The word was: " + currentWord.ToUpper();
                    DisableAllLetterButtons();
                }
                else
                {
                    // TWIST: shift category based on wrong guess count
                    string newCategory = GetCategoryForWrongCount(wrongGuesses);

                    if (newCategory != currentCategory)
                    {
                        string[] pool = newCategory == "Medium" ? mediumWords : hardWords;
                        currentWord = GetRandomWord(pool);
                        currentCategory = newCategory;

                        lblStatus.ForeColor = Color.DarkOrange;
                        lblStatus.Text = $"Category shifted to {newCategory}! New word chosen.";
                    }
                }
            }

            RefreshDisplay();
        }

        private void DisableAllLetterButtons()
        {
            foreach (var btn in letterButtons.Values)
            {
                btn.Enabled = false;
            }
        }

        private string GetCategoryForWrongCount(int wrong)
        {
            if (wrong >= 4) return "Hard";
            if (wrong >= 2) return "Medium";
            return "Easy";
        }

        private string GetRandomWord(string[] pool)
        {
            return pool[rand.Next(pool.Length)];
        }

        private void RefreshDisplay()
        {
            lblCategory.Text = "Category: " + currentCategory;
            lblHangmanArt.Text = GetHangmanArt(wrongGuesses);

            string display = string.Join(" ", currentWord.Select(c => guessedLetters.Contains(c) ? c.ToString().ToUpper() : "_"));
            lblWordDisplay.Text = display;

            string wrongLetters = string.Join(", ", guessedLetters.Where(c => !currentWord.Contains(c)).Select(c => c.ToString().ToUpper()));
            lblWrongLetters.Text = "Wrong guesses: " + (wrongLetters.Length > 0 ? wrongLetters : "none");

            lblRemaining.Text = "Remaining tries: " + (MaxWrongGuesses - wrongGuesses);
        }

        private string GetHangmanArt(int wrong)
        {
            string[] stages = new string[]
            {
                "  +---+\n      |\n      |\n      |\n     ===",
                "  +---+\n  O   |\n      |\n      |\n     ===",
                "  +---+\n  O   |\n  |   |\n      |\n     ===",
                "  +---+\n  O   |\n /|   |\n      |\n     ===",
                "  +---+\n  O   |\n /|\\  |\n      |\n     ===",
                "  +---+\n  O   |\n /|\\  |\n /    |\n     ===",
                "  +---+\n  O   |\n /|\\  |\n / \\  |\n     ==="
            };

            int index = Math.Min(wrong, stages.Length - 1);
            return stages[index];
        }
    }
}
