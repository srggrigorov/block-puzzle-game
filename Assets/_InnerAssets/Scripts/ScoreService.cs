using System;

public class ScoreService
{
    public Action<int> OnScoreChanged;
    public int Score { get; private set; }

    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }
}