using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // Ăn sao -> Cộng 10 điểm
        if (other.CompareTag("Star"))
        {
            GameManager.Instance.AddScore(10);
            Destroy(other.gameObject);
        }
        // Đâm thiên thạch -> Trừ 5 điểm và chuyển GameOver
        else if (other.CompareTag("Asteroid"))
        {
            GameManager.Instance.DeductScore(5);
            Destroy(other.gameObject);
            GameManager.Instance.GameOver();
        }
    }
}