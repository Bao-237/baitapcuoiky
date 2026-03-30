using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelProgressManager
{
    private const string LevelCompletedKeyPrefix = "level_completed_";
    private const string HighestCompletedLevelKey = "highest_completed_level";
    private const string CurrentLevelToPlayKey = "current_level_to_play";

    public static void MarkLevelCompleted(int levelNumber)
    {
        if (levelNumber <= 0)
        {
            return;
        }

        PlayerPrefs.SetInt(GetLevelCompletedKey(levelNumber), 1);

        int highestCompleted = PlayerPrefs.GetInt(HighestCompletedLevelKey, 0);
        if (levelNumber > highestCompleted)
        {
            PlayerPrefs.SetInt(HighestCompletedLevelKey, levelNumber);
        }

        int nextLevel = levelNumber + 1;
        int maxBuildIndex = Mathf.Max(1, SceneManager.sceneCountInBuildSettings - 1);
        nextLevel = Mathf.Clamp(nextLevel, 1, maxBuildIndex);
        PlayerPrefs.SetInt(CurrentLevelToPlayKey, nextLevel);

        PlayerPrefs.Save();
    }

    public static int GetHighestCompletedLevel()
    {
        if (PlayerPrefs.HasKey(HighestCompletedLevelKey))
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(HighestCompletedLevelKey, 0));
        }

        int highestCompleted = 0;
        int maxBuildIndex = Mathf.Max(1, SceneManager.sceneCountInBuildSettings - 1);

        for (int i = 1; i <= maxBuildIndex; i++)
        {
            if (IsLevelCompleted(i))
            {
                highestCompleted = i;
            }
        }

        PlayerPrefs.SetInt(HighestCompletedLevelKey, highestCompleted);
        PlayerPrefs.Save();

        return highestCompleted;
    }

    public static int GetNextLevelToPlay(int firstGameplayBuildIndex = 1)
    {
        int minLevel = Mathf.Max(1, firstGameplayBuildIndex);
        int maxBuildIndex = Mathf.Max(minLevel, SceneManager.sceneCountInBuildSettings - 1);

        int nextLevel;
        if (PlayerPrefs.HasKey(CurrentLevelToPlayKey))
        {
            nextLevel = PlayerPrefs.GetInt(CurrentLevelToPlayKey, minLevel);
        }
        else
        {
            nextLevel = GetHighestCompletedLevel() + 1;
        }

        nextLevel = Mathf.Clamp(nextLevel, minLevel, maxBuildIndex);
        PlayerPrefs.SetInt(CurrentLevelToPlayKey, nextLevel);
        PlayerPrefs.Save();

        return nextLevel;
    }

    public static int GetMaxUnlockedLevel(int firstGameplayBuildIndex = 1)
    {
        int minLevel = Mathf.Max(1, firstGameplayBuildIndex);
        int maxBuildIndex = Mathf.Max(minLevel, SceneManager.sceneCountInBuildSettings - 1);
        int unlocked = GetHighestCompletedLevel() + 1;

        return Mathf.Clamp(unlocked, minLevel, maxBuildIndex);
    }

    public static void SetCurrentLevelToPlay(int levelNumber, int firstGameplayBuildIndex = 1)
    {
        int minLevel = Mathf.Max(1, firstGameplayBuildIndex);
        int maxBuildIndex = Mathf.Max(minLevel, SceneManager.sceneCountInBuildSettings - 1);
        int clampedLevel = Mathf.Clamp(levelNumber, minLevel, maxBuildIndex);

        PlayerPrefs.SetInt(CurrentLevelToPlayKey, clampedLevel);
        PlayerPrefs.Save();
    }

    public static bool IsLevelCompleted(int levelNumber)
    {
        if (levelNumber <= 0)
        {
            return false;
        }

        return PlayerPrefs.GetInt(GetLevelCompletedKey(levelNumber), 0) == 1;
    }

    private static string GetLevelCompletedKey(int levelNumber)
    {
        return LevelCompletedKeyPrefix + levelNumber;
    }
}
