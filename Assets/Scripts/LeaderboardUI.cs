using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [Serializable]
    public class MockEntryInput
    {
        public string name;
        public int score;
    }

    [Serializable]
    public class LeaderboardRowUI
    {
        public TMP_Text rankText;
        public TMP_Text nameText;
        public TMP_Text scoreText;
    }

    [Serializable]
    private class LeaderboardEntry
    {
        public string id;
        public string name;
        public int score;
        public bool isPlayer;
    }

    [Serializable]
    private class LeaderboardSaveData
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    private const string LeaderboardSaveKey = "leaderboard_save_data_v1";

    [Header("Player")]
    public string playerId = "player";
    public string playerName = "You";

    [Header("Initial Mock Data")]
    public MockEntryInput[] initialMockEntries = new MockEntryInput[5];

    [Header("Top 5 Row TMP")]
    public LeaderboardRowUI[] topRows = new LeaderboardRowUI[5];

    private LeaderboardSaveData cache;

    private void OnEnable()
    {
        RefreshLeaderboardFromProgress();
    }

    public void RefreshLeaderboardFromProgress()
    {
        cache = LoadOrCreateData();

        int totalScore = LevelScoreManager.GetTotalBestScore();
        UpsertPlayerEntry(cache, totalScore);

        SortEntries(cache.entries);
        SaveData(cache);
        RenderTopRows(cache.entries);
    }

    [ContextMenu("Reset Leaderboard From Mock Data")]
    public void ResetLeaderboardFromMockData()
    {
        cache = CreateFromMockData();
        SaveData(cache);
        RefreshLeaderboardFromProgress();
    }

    private LeaderboardSaveData LoadOrCreateData()
    {
        if (PlayerPrefs.HasKey(LeaderboardSaveKey))
        {
            string json = PlayerPrefs.GetString(LeaderboardSaveKey);
            if (!string.IsNullOrEmpty(json))
            {
                LeaderboardSaveData loaded = JsonUtility.FromJson<LeaderboardSaveData>(json);
                if (loaded != null && loaded.entries != null)
                {
                    return loaded;
                }
            }
        }

        return CreateFromMockData();
    }

    private LeaderboardSaveData CreateFromMockData()
    {
        LeaderboardSaveData data = new LeaderboardSaveData();

        if (initialMockEntries == null)
        {
            return data;
        }

        for (int i = 0; i < initialMockEntries.Length; i++)
        {
            MockEntryInput input = initialMockEntries[i];
            if (input == null || string.IsNullOrEmpty(input.name))
            {
                continue;
            }

            data.entries.Add(new LeaderboardEntry
            {
                id = "mock_" + i,
                name = input.name,
                score = Mathf.Max(0, input.score),
                isPlayer = false
            });
        }

        SortEntries(data.entries);
        return data;
    }

    private void UpsertPlayerEntry(LeaderboardSaveData data, int score)
    {
        if (data == null)
        {
            return;
        }

        LeaderboardEntry playerEntry = null;
        for (int i = 0; i < data.entries.Count; i++)
        {
            if (data.entries[i] != null && data.entries[i].id == playerId)
            {
                playerEntry = data.entries[i];
                break;
            }
        }

        if (playerEntry == null)
        {
            playerEntry = new LeaderboardEntry
            {
                id = playerId,
                name = playerName,
                score = Mathf.Max(0, score),
                isPlayer = true
            };

            data.entries.Add(playerEntry);
            return;
        }

        playerEntry.name = playerName;
        playerEntry.score = Mathf.Max(0, score);
        playerEntry.isPlayer = true;
    }

    private static void SortEntries(List<LeaderboardEntry> entries)
    {
        entries.Sort((a, b) =>
        {
            int scoreCompare = b.score.CompareTo(a.score);
            if (scoreCompare != 0)
            {
                return scoreCompare;
            }

            return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static void SaveData(LeaderboardSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(LeaderboardSaveKey, json);
        PlayerPrefs.Save();
    }

    private void RenderTopRows(List<LeaderboardEntry> sortedEntries)
    {
        if (topRows == null)
        {
            return;
        }

        for (int i = 0; i < topRows.Length; i++)
        {
            LeaderboardRowUI row = topRows[i];
            if (row == null)
            {
                continue;
            }

            LeaderboardEntry entry = i < sortedEntries.Count ? sortedEntries[i] : null;

            if (row.rankText != null)
            {
                row.rankText.text = (i + 1).ToString();
            }

            if (row.nameText != null)
            {
                row.nameText.text = entry != null ? entry.name : "-";
            }

            if (row.scoreText != null)
            {
                row.scoreText.text = entry != null ? entry.score.ToString() : "0";
            }
        }
    }
}
