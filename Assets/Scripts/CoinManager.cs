using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    private const string CoinsKey = "coins";

    public static CoinManager Instance { get; private set; }

    public event Action<int> CoinsChanged;

    public int Coins { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (FindObjectOfType<CoinManager>() != null)
        {
            return;
        }

        GameObject coinManagerObject = new GameObject("CoinManager");
        coinManagerObject.AddComponent<CoinManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Coins = PlayerPrefs.GetInt(CoinsKey, 0);
        CoinsChanged?.Invoke(Coins);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Coins += amount;
        Save();
    }

    public bool SpendCoins(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        if (Coins < amount)
        {
            return false;
        }

        Coins -= amount;
        Save();
        return true;
    }

    private void Save()
    {
        PlayerPrefs.SetInt(CoinsKey, Coins);
        PlayerPrefs.Save();
        CoinsChanged?.Invoke(Coins);
    }
}