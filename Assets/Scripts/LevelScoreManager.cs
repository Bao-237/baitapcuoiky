using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelScoreManager
{
    private const string LevelBestScoreKeyPrefix = "level_best_score_";

    public static int RegisterLevelScore(int levelNumber, int score)
    {
        if (levelNumber <= 0)
        {
            return GetTotalBestScore();
        }

        int clampedScore = Mathf.Max(0, score);
        int currentBest = GetLevelBestScore(levelNumber);

        if (clampedScore > currentBest)
        {
            PlayerPrefs.SetInt(GetLevelBestScoreKey(levelNumber), clampedScore);
            PlayerPrefs.Save();
        }

        return GetTotalBestScore();
    }

    public static int GetLevelBestScore(int levelNumber)
    {
        if (levelNumber <= 0)
        {
            return 0;
        }

        return PlayerPrefs.GetInt(GetLevelBestScoreKey(levelNumber), 0);
    }

    public static int GetTotalBestScore()
    {
        int total = 0;
        int maxBuildIndex = Mathf.Max(1, SceneManager.sceneCountInBuildSettings - 1);

        for (int level = 1; level <= maxBuildIndex; level++)
        {
            total += GetLevelBestScore(level);
        }

        return total;
    }

    private static string GetLevelBestScoreKey(int levelNumber)
    {
        return LevelBestScoreKeyPrefix + levelNumber;
    }
}
