using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AchievementClaimUI : MonoBehaviour
{
    [System.Serializable]
    public class AchievementItem
    {
        public string id;
        public string title;
        public int requiredLevel;
        public TMP_Text titleText;
        public Button claimButton;
        public TMP_Text claimButtonText;
        public int rewardCoins;
    }

    private const string ClaimedKeyPrefix = "achievement_claimed_";
    private const string CoinsKey = "coins";

    [Header("Achievements")]
    public AchievementItem[] items;

    private UnityAction[] claimActions;

    private void Awake()
    {
        BindClaimButtons();
    }

    private void OnDestroy()
    {
        UnbindClaimButtons();
    }

    private void OnEnable()
    {
        BindClaimButtons();
        RefreshAll();
    }

    private void BindClaimButtons()
    {
        if (items == null)
        {
            return;
        }

        if (claimActions == null || claimActions.Length != items.Length)
        {
            claimActions = new UnityAction[items.Length];
        }

        for (int i = 0; i < items.Length; i++)
        {
            AchievementItem item = items[i];
            if (item == null || item.claimButton == null)
            {
                continue;
            }

            if (claimActions[i] == null)
            {
                int index = i;
                claimActions[i] = () => ClaimByIndex(index);
            }

            item.claimButton.onClick.RemoveListener(claimActions[i]);
            item.claimButton.onClick.AddListener(claimActions[i]);
        }
    }

    private void UnbindClaimButtons()
    {
        if (items == null || claimActions == null)
        {
            return;
        }

        for (int i = 0; i < items.Length; i++)
        {
            AchievementItem item = items[i];
            if (item == null || item.claimButton == null || claimActions[i] == null)
            {
                continue;
            }

            item.claimButton.onClick.RemoveListener(claimActions[i]);
        }
    }

    public void RefreshAll()
    {
        if (items == null)
        {
            return;
        }

        for (int i = 0; i < items.Length; i++)
        {
            RefreshItem(items[i], i);
        }
    }

    public void ClaimByIndex(int index)
    {
        if (items == null || index < 0 || index >= items.Length)
        {
            return;
        }

        AchievementItem item = items[index];
        if (item == null)
        {
            return;
        }

        string claimId = GetClaimId(item, index);

        if (!IsUnlocked(item) || IsClaimed(claimId))
        {
            return;
        }

        if (item.rewardCoins > 0)
        {
            bool added = AddCoinsSafe(item.rewardCoins);
            if (!added)
            {
                Debug.LogWarning("Could not add achievement coins.");
                return;
            }
        }

        SetClaimed(claimId, true);

        RefreshItem(item, index);
    }

    private void RefreshItem(AchievementItem item, int index)
    {
        if (item == null)
        {
            return;
        }

        bool unlocked = IsUnlocked(item);
        bool claimed = IsClaimed(GetClaimId(item, index));

        if (item.titleText != null)
        {
            item.titleText.text = item.title;
        }

        if (item.claimButton != null)
        {
            item.claimButton.interactable = unlocked && !claimed;
        }

        TMP_Text buttonLabel = item.claimButtonText;

        if (buttonLabel == null && item.claimButton != null)
        {
            buttonLabel = item.claimButton.GetComponentInChildren<TMP_Text>(true);
        }

        string buttonText;
        if (claimed)
        {
            buttonText = "Claimed";
        }
        else if (!unlocked)
        {
            buttonText = "Locked";
        }
        else
        {
            buttonText = "Claim +" + item.rewardCoins;
        }

        if (buttonLabel != null)
        {
            buttonLabel.text = buttonText;
        }
    }

    private bool IsUnlocked(AchievementItem item)
    {
        return LevelProgressManager.IsLevelCompleted(item.requiredLevel);
    }

    private static string GetClaimId(AchievementItem item, int fallbackIndex)
    {
        if (item != null && !string.IsNullOrEmpty(item.id))
        {
            return item.id;
        }

        return "index_" + fallbackIndex;
    }

    private static bool AddCoinsSafe(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        EnsureCoinManagerExists();

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddCoins(amount);
            return true;
        }

        int currentCoins = PlayerPrefs.GetInt(CoinsKey, 0);
        PlayerPrefs.SetInt(CoinsKey, currentCoins + amount);
        PlayerPrefs.Save();
        return true;
    }

    private static void EnsureCoinManagerExists()
    {
        if (CoinManager.Instance != null)
        {
            return;
        }

        CoinManager existingManager = Object.FindObjectOfType<CoinManager>();
        if (existingManager != null)
        {
            return;
        }

        GameObject coinManagerObject = new GameObject("CoinManager");
        coinManagerObject.AddComponent<CoinManager>();
    }

    private static bool IsClaimed(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        return PlayerPrefs.GetInt(ClaimedKeyPrefix + id, 0) == 1;
    }

    private static void SetClaimed(string id, bool claimed)
    {
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        PlayerPrefs.SetInt(ClaimedKeyPrefix + id, claimed ? 1 : 0);
        PlayerPrefs.Save();
    }
}
