using UnityEngine;

public enum PowerUpType
{
    Shield,
    WeaponUpgrade,
    Health,
    Magnet
}

public class PowerUpItem : MonoBehaviour
{
    [Header("Item Type")]
    public PowerUpType itemType = PowerUpType.Shield;

    [Header("Movement Settings")]
    public float fallSpeed = 3f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Tự động gán Tag tương ứng với loại vật phẩm để Player va chạm nhận diện được
        switch (itemType)
        {
            case PowerUpType.Shield:
                gameObject.tag = "Shield";
                break;
            case PowerUpType.WeaponUpgrade:
                gameObject.tag = "WeaponUpgrade";
                break;
            case PowerUpType.Health:
                gameObject.tag = "Health";
                break;
            case PowerUpType.Magnet:
                gameObject.tag = "Magnet";
                break;
        }

        // Rơi xuống bằng vận tốc vật lý
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.down * fallSpeed;
        }

        // Tự hủy sau 7 giây
        Destroy(gameObject, 7f);
    }

    void Update()
    {
        // Dự phòng nếu Rigidbody2D ở dạng Kinematic
        if (rb == null || rb.bodyType == RigidbodyType2D.Kinematic)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
        }
    }
}