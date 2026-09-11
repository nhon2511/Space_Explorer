using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("Shield Configuration")]
    public GameObject shieldVisual; // Đối tượng vòng khiên quanh tàu (ShieldVisual)
    private bool hasShield = false;

    void Start()
    {
        // Mặc định lúc mới vào game chưa có khiên
        hasShield = false;
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // TRƯỜNG HỢP 1: Nhặt được Item Khiên rơi
        if (other.CompareTag("Shield"))
        {
            ActivateShield();

            // Phát âm thanh nhặt khiên
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShieldPickup();
            }

            Destroy(other.gameObject); // Xóa item trên màn hình
        }
        // TRƯỜNG HỢP 2: Nhặt được Ngôi sao (Star)
        else if (other.CompareTag("Star"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(10);
            }

            // Phát âm thanh ăn sao
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayStar();
            }

            Destroy(other.gameObject);
        }
        // TRƯỜNG HỢP 3: Va chạm với Thiên thạch (Asteroid)
        else if (other.CompareTag("Asteroid"))
        {
            if (hasShield)
            {
                // ĐANG CÓ KHIÊN:
                // Khiên chịu đòn -> Vỡ khiên, nổ thiên thạch, tàu KHÔNG bị trừ điểm và KHÔNG chết
                BreakShield();

                // Phát âm thanh vỡ khiên kèm tiếng nổ
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayShieldBreak();
                    AudioManager.Instance.PlayExplosion();
                }

                // Rung màn hình vừa phải khi khiên vỡ đỡ đòn
                if (CameraShake.Instance != null)
                {
                    CameraShake.Instance.Shake(0.2f, 0.25f);
                }

                Destroy(other.gameObject);
            }
            else
            {
                // KHÔNG CÓ KHIÊN:
                // Phát âm thanh nổ khi tàu bị va chạm
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayExplosion();
                }

                // Rung màn hình thật mạnh khi tàu bị tiêu diệt
                if (CameraShake.Instance != null)
                {
                    CameraShake.Instance.Shake(0.35f, 0.45f);
                }

                // Trừ 5 điểm và chuyển màn hình GameOver
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DeductScore(5);
                    GameManager.Instance.GameOver();
                }

                Destroy(other.gameObject);
            }
        }
    }

    // Bật trạng thái khiên
    void ActivateShield()
    {
        hasShield = true;
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }
    }

    // Phá vỡ khiên
    void BreakShield()
    {
        hasShield = false;
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
    }
}