using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class ui_mainmenu : MonoBehaviour
{
    [Header("Coin UI")]
    public TMP_Text coinText;

    void Start()
    {
        RefreshCoinsUI();

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.CoinsChanged += OnCoinsChanged;
        }
    }

    void OnDestroy()
    {
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.CoinsChanged -= OnCoinsChanged;
        }
    }

    public void LoadLevel(int n)
    {
        SceneManager.LoadScene(n);
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

        UpdateCoinText(0);
    }

    private void UpdateCoinText(int coins)
    {
        if (coinText != null)
        {
            coinText.text = coins + " xu";
        }
    }
}
