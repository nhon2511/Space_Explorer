using System.Collections;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("Shield Configuration")]
    public GameObject shieldVisual; // Đối tượng vòng khiên quanh tàu (ShieldVisual)
    private bool hasShield = false;

    [Header("Invulnerability Settings (Chớp nháy bất tử)")]
    public float invulnerableDuration = 1.5f; // Thời gian bất tử sau khi trúng đòn
    public float flashInterval = 0.15f;       // Tốc độ chớp nháy
    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Mặc định ban đầu chưa có khiên
        hasShield = false;
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. NHẶT VẬT PHẨM KHIÊN
        if (other.CompareTag("Shield"))
        {
            ActivateShield();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayShieldPickup();

            Destroy(other.gameObject);
        }
        // 2. NHẶT NGÔI SAO
        else if (other.CompareTag("Star"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(10);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayStar();

            Destroy(other.gameObject);
        }
        // 3. NHẶT ITEM NÂNG CẤP VŨ KHÍ
        else if (other.CompareTag("WeaponUpgrade"))
        {
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null) pc.UpgradeWeapon();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayShieldPickup(); // Tái sử dụng âm thanh nhặt đồ

            Destroy(other.gameObject);
        }
        // 4. NHẶT ITEM HỒI MÁU / MẠNG
        else if (other.CompareTag("Health"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.Heal(1);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayShieldPickup();

            Destroy(other.gameObject);
        }
        // 5. NHẶT ITEM NAM CHÂM HÚT SAO
        else if (other.CompareTag("Magnet"))
        {
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null) pc.ActivateMagnet(7f);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayShieldPickup();

            Destroy(other.gameObject);
        }
        // 6. VA CHẠM THIÊN THẠCH
        else if (other.CompareTag("Asteroid"))
        {
            if (hasShield)
            {
                // CÓ KHIÊN: Vỡ khiên đỡ đòn, tàu không mất mạng
                BreakShield();

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayShieldBreak();
                    AudioManager.Instance.PlayExplosion();
                }

                if (CameraShake.Instance != null)
                    CameraShake.Instance.Shake(0.2f, 0.25f);

                Destroy(other.gameObject);
            }
            else
            {
                // KHÔNG CÓ KHIÊN: Kiểm tra xem có đang trong thời gian bất tử (I-Frames) không
                if (isInvulnerable) return;

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayExplosion();

                if (CameraShake.Instance != null)
                    CameraShake.Instance.Shake(0.3f, 0.4f);

                // Trừ điểm và trừ 1 mạng
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DeductScore(5);
                    bool isDead = GameManager.Instance.TakeDamage(1);

                    // Nếu chưa hết mạng -> Bật trạng thái chớp nháy bất tử
                    if (!isDead)
                    {
                        StartCoroutine(InvulnerabilityRoutine());
                    }
                }

                Destroy(other.gameObject);
            }
        }
    }

    // Bật khiên
    public void ActivateShield()
    {
        hasShield = true;
        if (shieldVisual != null)
            shieldVisual.SetActive(true);
    }

    // Vỡ khiên
    public void BreakShield()
    {
        hasShield = false;
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    // Hiệu ứng chớp nháy trong suốt khi bất tử
    IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        float elapsed = 0f;

        while (elapsed < invulnerableDuration)
        {
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = (c.a == 1f) ? 0.3f : 1f; // Nhấp nháy mờ/rõ
                spriteRenderer.color = c;
            }

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // Trả màu sắc về bình thường
        if (spriteRenderer != null)
        {
            Color normalColor = spriteRenderer.color;
            normalColor.a = 1f;
            spriteRenderer.color = normalColor;
        }

        isInvulnerable = false;
    }
}