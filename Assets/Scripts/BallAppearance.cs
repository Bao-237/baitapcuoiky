using UnityEngine;

/// <summary>
/// Gắn script này vào Ball GameObject trong Play scene.
/// Khi scene load, tự động đọc từ BallShopManager và áp
/// Material + kích thước của ball đang được trang bị.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class BallAppearance : MonoBehaviour
{
    [Header("Scale gốc của Ball (không áp sizeMultiplier)")]
    public Vector3 baseScale = Vector3.one;

    private void Start()
    {
        ApplyEquippedBall();
    }

    /// <summary>Áp appearance của ball đang trang bị. Có thể gọi lại bất cứ lúc nào.</summary>
    public void ApplyEquippedBall()
    {
        if (BallShopManager.Instance == null)
        {
            Debug.LogWarning("BallAppearance: BallShopManager.Instance chưa tồn tại.");
            return;
        }

        BallData equipped = BallShopManager.Instance.GetEquippedBall();

        if (equipped == null)
        {
            // Không có gì được trang bị, giữ nguyên mặc định
            return;
        }

        // Áp Material
        if (equipped.ballMaterial != null)
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = equipped.ballMaterial;
            }
        }

        // Áp kích thước
        transform.localScale = baseScale * equipped.sizeMultiplier;
    }
}
