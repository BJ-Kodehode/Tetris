using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TetrisAvalonia.Models;

namespace TetrisAvalonia.Utilities
{
    public static class HighscoreManager
    {
        private static readonly string HighscoreFilePath = "highscores.json";

        public static List<Highscore> LoadHighscores()
        {
            if (!File.Exists(HighscoreFilePath))
            {
                Console.WriteLine("Highscore file not found. Returning an empty list.");
                return new List<Highscore>();
            }

            var json = File.ReadAllText(HighscoreFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("Highscore file is empty. Returning an empty list.");
                return new List<Highscore>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<Highscore>>(json) ?? new List<Highscore>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Failed to deserialize highscores: {ex.Message}. Returning an empty list.");
                return new List<Highscore>();
            }
        }

        public static void SaveHighscores(List<Highscore> highscores)
        {
            var json = JsonSerializer.Serialize(highscores);
            File.WriteAllText(HighscoreFilePath, json);
        }

        public static void AddHighscore(string playerName, int score)
        {
            var highscores = LoadHighscores();
            highscores.Add(new Highscore(playerName, score));
            highscores.Sort((a, b) => b.Score.CompareTo(a.Score)); // Sort descending by score

            if (highscores.Count > 10) // Keep top 10 scores
            {
                highscores.RemoveAt(highscores.Count - 1);
            }

            SaveHighscores(highscores);
        }
    }
}