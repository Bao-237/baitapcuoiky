using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI của Shop Ball. Attach script này vào Panel Shop trong Canvas.
///
/// Hierarchy mẫu:
///   ShopPanel
///     ├── CoinDisplay (TMP_Text)         ← hiển thị coins hiện tại
///     ├── ScrollView > Content           ← chứa các BallItemUI (kéo thả)
///     └── PopupCannotAfford (GameObject) ← thông báo không đủ tiền
/// </summary>
public class BallShopUI : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // Inspector fields
    // ──────────────────────────────────────────────

    [Header("Coin Display")]
    [SerializeField] private TMP_Text coinText;

    [Header("Item Prefab & Container")]
    [SerializeField] private GameObject ballItemPrefab;  // Prefab 1 ô shop
    [SerializeField] private Transform  itemContainer;   // ScrollView/Content

    [Header("Thông báo không đủ coins")]
    [SerializeField] private GameObject popupCannotAfford;
    [SerializeField] private float      popupDuration = 2f;

    [Header("Thông báo mua thành công")]
    [SerializeField] private GameObject popupPurchased;

    // ──────────────────────────────────────────────
    // Private state
    // ──────────────────────────────────────────────

    private System.Action<int> coinChangedListener;
    private float             popupAffordTimer;
    private float             popupPurchasedTimer;
    private bool              coinManagerSubscribed = false;

    // ──────────────────────────────────────────────
    // Lifecycle
    // ──────────────────────────────────────────────

    private void Awake()
    {
        coinChangedListener = (coins) => RefreshCoinDisplay(coins);
    }

    private void OnEnable()
    {
        coinManagerSubscribed = false;

        SubscribeToCoinManager();

        if (BallShopManager.Instance != null)
        {
            BallShopManager.Instance.OnBallPurchased += HandleBallPurchased;
            BuildItemList();
        }

        HidePopups();
    }

    private void SubscribeToCoinManager()
    {
        if (coinManagerSubscribed) return;
        if (CoinManager.Instance == null) return;

        CoinManager.Instance.CoinsChanged -= coinChangedListener;
        CoinManager.Instance.CoinsChanged += coinChangedListener;
        RefreshCoinDisplay(CoinManager.Instance.Coins);
        coinManagerSubscribed = true;

        if (coinText == null)
            Debug.LogWarning("[BallShopUI] coinText chưa được gán trong Inspector! Kéo TMP_Text vào trường Coin Text.");
    }

    /// <summary>
    /// Start() đảm bảo BuildItemList chạy SAU tất cả Awake() đã xong.
    /// Quan trọng vì OnEnable() có thể chạy trước BallShopManager.Awake()
    /// khiến Instance vẫn còn null tại thời điểm đó.
    /// </summary>
    private void Start()
    {
        // Đăng ký event nếu OnEnable chạy trước BallShopManager.Awake
        if (BallShopManager.Instance != null)
        {
            BallShopManager.Instance.OnBallPurchased -= HandleBallPurchased;
            BallShopManager.Instance.OnBallPurchased += HandleBallPurchased;
        }

        // Thử subscribe lại nếu OnEnable chạy trước CoinManager.Awake
        SubscribeToCoinManager();

        // Build lại sau khi chắc chắn BallShopManager đã Awake xong
        BuildItemList();
    }

    private void OnDisable()
    {
        if (CoinManager.Instance != null)
            CoinManager.Instance.CoinsChanged -= coinChangedListener;

        if (BallShopManager.Instance != null)
            BallShopManager.Instance.OnBallPurchased -= HandleBallPurchased;
    }

    private void Update()
    {
        // Fallback: thử đăng ký CoinManager nếu chưa kịp (do thứ tự Awake)
        if (!coinManagerSubscribed)
            SubscribeToCoinManager();

        // Tự ẩn popup sau thời gian
        if (popupCannotAfford != null && popupCannotAfford.activeSelf)
        {
            popupAffordTimer -= Time.unscaledDeltaTime;
            if (popupAffordTimer <= 0f)
                popupCannotAfford.SetActive(false);
        }

        if (popupPurchased != null && popupPurchased.activeSelf)
        {
            popupPurchasedTimer -= Time.unscaledDeltaTime;
            if (popupPurchasedTimer <= 0f)
                popupPurchased.SetActive(false);
        }
    }

    // ──────────────────────────────────────────────
    // Build UI
    // ──────────────────────────────────────────────

    /// <summary>Tạo/cập nhật danh sách ô Ball từ BallShopManager.</summary>
    public void BuildItemList()
    {
        if (itemContainer == null || ballItemPrefab == null) return;
        if (BallShopManager.Instance == null || BallShopManager.Instance.allBalls == null) return;

        // Xoá item cũ
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        BallData[] balls = BallShopManager.Instance.allBalls;

        for (int i = 0; i < balls.Length; i++)
        {
            BallData ball = balls[i];
            if (ball == null) continue;

            GameObject itemGO = Instantiate(ballItemPrefab, itemContainer);
            itemGO.name = "BallItem_" + ball.ballId;

            SetupItemUI(itemGO, ball);
        }
    }

    // ──────────────────────────────────────────────
    // Setup từng item
    // ──────────────────────────────────────────────

    /// <summary>
    /// Gán dữ liệu và sự kiện cho 1 ô Ball trong Shop.
    ///
    /// Prefab cần có các component con với tên:
    ///   "BallPreview"    — Image (ảnh xem trước)
    ///   "BallName"       — TMP_Text (tên ball)
    ///   "BallDesc"       — TMP_Text (mô tả)
    ///   "BallPrice"      — TMP_Text (giá)
    ///   "ActionButton"   — Button (Mua / Trang bị / Đang dùng)
    ///   "ActionText"     — TMP_Text (nhãn nút)
    ///   "EquippedBadge"  — GameObject (badge "Đang dùng")
    /// </summary>
    private void SetupItemUI(GameObject itemGO, BallData ball)
    {
        // Preview
        Image preview = itemGO.transform.Find("BallPreview")?.GetComponent<Image>();
        if (preview != null && ball.previewSprite != null)
            preview.sprite = ball.previewSprite;

        // Tên
        TMP_Text nameText = itemGO.transform.Find("BallName")?.GetComponent<TMP_Text>();
        if (nameText != null)
            nameText.text = ball.ballName;

        // Mô tả
        TMP_Text descText = itemGO.transform.Find("BallDesc")?.GetComponent<TMP_Text>();
        if (descText != null)
            descText.text = ball.description;

        // Giá
        TMP_Text priceText = itemGO.transform.Find("BallPrice")?.GetComponent<TMP_Text>();
        if (priceText != null)
            priceText.text = ball.isDefault ? "Miễn phí" : ball.price + " 🪙";

        // Nút hành động
        Button actionBtn = itemGO.transform.Find("ActionButton")?.GetComponent<Button>();
        TMP_Text actionText = itemGO.transform.Find("ActionText")?.GetComponent<TMP_Text>() ??
                              actionBtn?.GetComponentInChildren<TMP_Text>();

        // Badge đang dùng
        GameObject equippedBadge = itemGO.transform.Find("EquippedBadge")?.gameObject;

        RefreshItemButton(ball, actionBtn, actionText, equippedBadge);

        if (actionBtn != null)
        {
            BallData capturedBall = ball;
            Button capturedBtn = actionBtn;
            TMP_Text capturedText = actionText;
            GameObject capturedBadge = equippedBadge;

            actionBtn.onClick.RemoveAllListeners();
            actionBtn.onClick.AddListener(() =>
            {
                HandleActionButton(capturedBall, capturedBtn, capturedText, capturedBadge);
            });
        }
    }

    // ──────────────────────────────────────────────
    // Logic nút
    // ──────────────────────────────────────────────

    private void HandleActionButton(BallData ball, Button btn, TMP_Text label, GameObject badge)
    {
        if (BallShopManager.Instance == null) return;

        if (!BallShopManager.Instance.IsOwned(ball.ballId))
        {
            // Chưa sở hữu → thử mua
            bool success = BallShopManager.Instance.Purchase(ball);
            if (!success)
            {
                ShowPopupCannotAfford();
            }
        }
        else
        {
            // Đã sở hữu → Trang bị
            BallShopManager.Instance.Equip(ball);
        }

        // Refresh toàn bộ danh sách để cập nhật badge & nút
        BuildItemList();
    }

    private void RefreshItemButton(BallData ball, Button btn, TMP_Text label, GameObject badge)
    {
        if (BallShopManager.Instance == null) return;

        bool owned    = BallShopManager.Instance.IsOwned(ball.ballId);
        BallData equipped = BallShopManager.Instance.GetEquippedBall();
        bool isEquipped = equipped != null && equipped.ballId == ball.ballId;

        if (badge != null)
            badge.SetActive(isEquipped);

        if (btn == null) return;

        if (isEquipped)
        {
            btn.interactable = false;
            if (label != null) label.text = "Đang dùng";
        }
        else if (owned)
        {
            btn.interactable = true;
            if (label != null) label.text = "Trang bị";
        }
        else
        {
            btn.interactable = true;
            if (label != null) label.text = ball.isDefault ? "Nhận" : "Mua " + ball.price + "🪙";
        }
    }

    // ──────────────────────────────────────────────
    // Callbacks
    // ──────────────────────────────────────────────

    private void HandleBallPurchased(BallData ball)
    {
        if (popupPurchased != null)
        {
            popupPurchased.SetActive(true);
            popupPurchasedTimer = popupDuration;
        }
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private void RefreshCoinDisplay(int coins)
    {
        if (coinText != null)
            coinText.text = "🪙 " + coins;
    }

    private void ShowPopupCannotAfford()
    {
        if (popupCannotAfford != null)
        {
            popupCannotAfford.SetActive(true);
            popupAffordTimer = popupDuration;
        }
    }

    private void HidePopups()
    {
        if (popupCannotAfford != null) popupCannotAfford.SetActive(false);
        if (popupPurchased   != null) popupPurchased.SetActive(false);
    }
}
