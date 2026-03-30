using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class ui_mainmenu : MonoBehaviour
{
    private const string CoinsKey = "coins";

    [Header("Coin UI")]
    public TMP_Text coinText;
    public TMP_Text achievementCoinText;
    public TMP_Text startMenuTotalScoreText;

    [Header("Menu Panels")]
    public GameObject startMenuPanel;
    public GameObject achievementPanel;
    public GameObject leaderboardPanel;

    [Header("Level Progress")]
    public int firstGameplayBuildIndex = 1;

    private bool coinEventsBound;

    void Start()
    {
        ShowStartMenu();
        TryBindCoinManager();
        RefreshCoinsUI();
        RefreshTotalScoreUI();
    }

    void Update()
    {
        if (!coinEventsBound)
        {
            TryBindCoinManager();
        }
    }

    void OnDestroy()
    {
        if (coinEventsBound && CoinManager.Instance != null)
        {
            CoinManager.Instance.CoinsChanged -= OnCoinsChanged;
        }

        coinEventsBound = false;
    }

    public void LoadLevel(int n)
    {
        SceneManager.LoadScene(n);
    }

    public void PlayContinue()
    {
        int targetLevel = LevelProgressManager.GetNextLevelToPlay(firstGameplayBuildIndex);
        SceneManager.LoadScene(targetLevel);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void WatchRewardedAdForCoins()
    {
        if (ads.Instance == null)
        {
            Debug.LogWarning("Ads manager not found in scene.");
            return;
        }

        ads.Instance.ShowRewardedAdForCoins();
    }

    public void OpenAchievement()
    {
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(false);
        }

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }

        if (achievementPanel != null)
        {
            achievementPanel.SetActive(true);
        }
    }

    public void OpenLeaderboard()
    {
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(false);
        }

        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);

            LeaderboardUI leaderboardUI = leaderboardPanel.GetComponent<LeaderboardUI>();
            if (leaderboardUI != null)
            {
                leaderboardUI.RefreshLeaderboardFromProgress();
            }
        }

        RefreshTotalScoreUI();
    }

    public void BackFromAchievement()
    {
        ShowStartMenu();
    }

    public void BackFromLeaderboard()
    {
        ShowStartMenu();
        RefreshTotalScoreUI();
    }

    private void ShowStartMenu()
    {
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(true);
        }

        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }

    private void OnCoinsChanged(int coins)
    {
        UpdateCoinText(coins);
    }

    private void RefreshCoinsUI()
    {
        if (CoinManager.Instance != null)
        {
            UpdateCoinText(CoinManager.Instance.Coins);
            return;
        }

        UpdateCoinText(PlayerPrefs.GetInt(CoinsKey, 0));
    }

    private void TryBindCoinManager()
    {
        if (coinEventsBound || CoinManager.Instance == null)
        {
            return;
        }

        CoinManager.Instance.CoinsChanged += OnCoinsChanged;
        coinEventsBound = true;
        UpdateCoinText(CoinManager.Instance.Coins);
    }

    private void UpdateCoinText(int coins)
    {
        if (coinText != null)
        {
            coinText.text = coins + " xu";
        }

        if (achievementCoinText != null)
        {
            achievementCoinText.text = coins + " xu";
        }
    }

    private void RefreshTotalScoreUI()
    {
        if (startMenuTotalScoreText == null)
        {
            return;
        }

        int totalScore = LevelScoreManager.GetTotalBestScore();
        startMenuTotalScoreText.text = totalScore.ToString();
    }
}
