using System;

namespace TetrisAvalonia.Models
{
    public class Highscore
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }

        public Highscore(string playerName, int score)
        {
            PlayerName = playerName;
            Score = score;
        }
    }
}