using System;
using UnityEngine;
using GoogleMobileAds.Api;

public class ads : MonoBehaviour
{
    public static ads Instance { get; private set; }

    [Header("Reward Config")]
    [SerializeField, Min(1)] private int rewardCoins = 50;
    [SerializeField] private bool useTestAds = false;

    [Header("Android Rewarded Ad Unit ID")]
    [SerializeField] private string androidRewardedAdUnitId = "ca-app-pub-1868095958078656/2408998539";

    private RewardedAd rewardedAd;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (FindObjectOfType<ads>() != null)
        {
            return;
        }

        GameObject adsObject = new GameObject("AdsManager");
        adsObject.AddComponent<ads>();
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
    }

    private void Start()
    {
#if !UNITY_ANDROID
        Debug.Log("Rewarded ads are configured for Android only.");
        return;
#endif

        MobileAds.Initialize(_ =>
        {
            LoadRewardedAd();
        });
    }

    private string RewardedAdUnitId
    {
        get
        {
#if UNITY_ANDROID
            return useTestAds ? "ca-app-pub-3940256099942544/5224354917" : androidRewardedAdUnitId;
#else
            return "unused";
#endif
        }
    }

    public void LoadRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        AdRequest adRequest = new AdRequest();

        RewardedAd.Load(RewardedAdUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogWarning("Rewarded ad failed to load: " + error);
                return;
            }

            rewardedAd = ad;
            RegisterRewardedAdEvents(rewardedAd);
            Debug.Log("Rewarded ad loaded.");
        });
    }

    public void ShowRewardedAdForCoins()
    {
        if (rewardedAd == null)
        {
            Debug.LogWarning("Rewarded ad is not loaded yet. Reloading...");
            LoadRewardedAd();
            return;
        }

        if (!rewardedAd.CanShowAd())
        {
            Debug.LogWarning("Rewarded ad cannot be shown now. Reloading...");
            LoadRewardedAd();
            return;
        }

        rewardedAd.Show((Reward reward) =>
        {
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.AddCoins(rewardCoins);
            }

            Debug.Log("Player earned reward: +" + rewardCoins + " coins.");
        });
    }

    public void SetRewardCoins(int amount)
    {
        rewardCoins = Mathf.Max(1, amount);
    }

    public int GetRewardCoins()
    {
        return rewardCoins;
    }

    private void RegisterRewardedAdEvents(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            LoadRewardedAd();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogWarning("Rewarded ad full screen failed: " + error);
            LoadRewardedAd();
        };
    }
}
