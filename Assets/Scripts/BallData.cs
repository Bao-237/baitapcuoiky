using UnityEngine;

/// <summary>
/// ScriptableObject chứa thông tin một loại Ball trong Shop.
/// Tạo bằng cách: chuột phải trong Project > Create > Ball Shop > Ball Data
/// </summary>
[CreateAssetMenu(fileName = "BallData", menuName = "Ball Shop/Ball Data", order = 0)]
public class BallData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string ballId;           // ID duy nhất, dùng để lưu PlayerPrefs
    public string ballName;         // Tên hiển thị
    [TextArea(2, 4)]
    public string description;      // Mô tả ngắn

    [Header("Hình ảnh")]
    public Sprite previewSprite;    // Ảnh xem trước trong shop
    public Material ballMaterial;   // Material áp lên ball khi được trang bị

    [Header("Giá & Trạng thái")]
    public int price;               // Giá (số coins)
    public bool isDefault;          // True = mặc định, không cần mua

    [Header("Thuộc tính đặc biệt (tuỳ chọn)")]
    public float sizeMultiplier = 1f;   // Hệ số kích thước
    public float bounceMultiplier = 1f; // Hệ số nảy
}
