using System;
using UnityEngine;

/// <summary>
/// Singleton quản lý toàn bộ logic Shop Ball:
/// - Kiểm tra ball đã sở hữu chưa
/// - Mua ball bằng CoinManager
/// - Trang bị ball (equip) và áp Material
/// </summary>
public class BallShopManager : MonoBehaviour
{
    private const string OwnedKeyPrefix = "ball_owned_";
    private const string EquippedKey    = "ball_equipped_id";

    public static BallShopManager Instance { get; private set; }

    [Header("Danh sách Ball trong Shop")]
    public BallData[] allBalls;

    public event Action<BallData> OnBallEquipped;
    public event Action<BallData> OnBallPurchased;

    // ──────────────────────────────────────────────
    // Lifecycle
    // ──────────────────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        // Chỉ tạo nếu scene không có sẵn BallShopManager nào
        if (FindObjectOfType<BallShopManager>() != null) return;

        GameObject go = new GameObject("BallShopManager");
        go.AddComponent<BallShopManager>();
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

        // Đảm bảo ball mặc định luôn được sở hữu
        if (allBalls != null)
        {
            foreach (var ball in allBalls)
            {
                if (ball != null && ball.isDefault && !string.IsNullOrEmpty(ball.ballId))
                {
                    SetOwned(ball.ballId, true);
                }
            }
        }
    }

    private void Start()
    {
        // Appearance được apply bởi BallAppearance.cs trong Play scene
    }

    // ──────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────

    /// <summary>Kiểm tra ball đã được sở hữu chưa.</summary>
    public bool IsOwned(string ballId)
    {
        if (string.IsNullOrEmpty(ballId)) return false;
        return PlayerPrefs.GetInt(OwnedKeyPrefix + ballId, 0) == 1;
    }

    /// <summary>Mua ball. Trả về true nếu thành công.</summary>
    public bool Purchase(BallData ball)
    {
        if (ball == null) return false;
        if (IsOwned(ball.ballId)) return false;

        if (CoinManager.Instance == null)
        {
            Debug.LogWarning("CoinManager không tồn tại!");
            return false;
        }

        bool success = CoinManager.Instance.SpendCoins(ball.price);
        if (!success)
        {
            Debug.Log("Không đủ coins để mua: " + ball.ballName);
            return false;
        }

        SetOwned(ball.ballId, true);
        Debug.Log("Đã mua: " + ball.ballName);

        OnBallPurchased?.Invoke(ball);

        // Tự động trang bị sau khi mua
        Equip(ball);
        return true;
    }

    /// <summary>Trang bị ball (đã sở hữu). Trả về true nếu thành công.</summary>
    public bool Equip(BallData ball)
    {
        if (ball == null) return false;
        if (!IsOwned(ball.ballId)) return false;

        PlayerPrefs.SetString(EquippedKey, ball.ballId);
        PlayerPrefs.Save();

        OnBallEquipped?.Invoke(ball);
        Debug.Log("Đã trang bị: " + ball.ballName);
        return true;
    }

    /// <summary>Lấy BallData đang được trang bị.</summary>
    public BallData GetEquippedBall()
    {
        string equippedId = PlayerPrefs.GetString(EquippedKey, "");
        if (string.IsNullOrEmpty(equippedId) || allBalls == null) return null;

        foreach (var ball in allBalls)
        {
            if (ball != null && ball.ballId == equippedId)
                return ball;
        }

        return null;
    }

    // ──────────────────────────────────────────────
    // Internal
    // ──────────────────────────────────────────────

    private void SetOwned(string ballId, bool owned)
    {
        PlayerPrefs.SetInt(OwnedKeyPrefix + ballId, owned ? 1 : 0);
        PlayerPrefs.Save();
    }
}
