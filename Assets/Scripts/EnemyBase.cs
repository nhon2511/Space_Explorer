using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 2;
    protected int currentHealth;
    public int scoreValue = 40;
    public int contactDamage = 1;

    [Header("VFX & SFX")]
    public GameObject explosionPrefab;
    public float flashDuration = 0.1f;

    [Header("Drop Settings")]
    public GameObject[] dropPrefabs;
    [Range(0f, 1f)]
    public float dropChance = 0.4f;

    [Header("Lifetime Settings")]
    public bool autoDestroy = true;
    public float autoDestroyTime = 15f;

    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    protected Transform playerTransform;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        // Tự động tìm vị trí Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        // Tự gán Tag Enemy
        gameObject.tag = "Enemy";

        // Tự hủy sau autoDestroyTime giây nếu là quái thường
        if (autoDestroy && autoDestroyTime > 0f)
        {
            Destroy(gameObject, autoDestroyTime);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Hiệu ứng chớp nháy khi trúng đạn
        StartCoroutine(FlashWhiteRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        // 1. Cộng điểm & EXP
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue, transform.position);
        }

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.AddExp(25);
        }

        // 2. Âm thanh nổ
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExplosion();
        }

        // 3. Rung màn hình
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.18f, 0.25f);
        }

        // 4. Sinh hiệu ứng nổ
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // 5. Tỉ lệ rơi vật phẩm
        SpawnDropItem();

        // 6. Hủy GameObject
        Destroy(gameObject);
    }

    private void SpawnDropItem()
    {
        if (Random.value > dropChance) return;

        // Ưu tiên rơi đồ thích ứng theo tình trạng người chơi
        if (ItemDropManager.Instance != null)
        {
            GameObject smartItem = ItemDropManager.Instance.GetSmartDrop();
            if (smartItem != null)
            {
                Instantiate(smartItem, transform.position, Quaternion.identity);
                return;
            }
        }

        if (dropPrefabs == null || dropPrefabs.Length == 0) return;

        int randIndex = Random.Range(0, dropPrefabs.Length);
        if (dropPrefabs[randIndex] != null)
        {
            Instantiate(dropPrefabs[randIndex], transform.position, Quaternion.identity);
        }
    }

    private IEnumerator FlashWhiteRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // Bị đạn Laser của Player bắn trúng
        if (other.CompareTag("Laser"))
        {
            PlayerLaser pl = other.GetComponent<PlayerLaser>();
            if (pl != null)
            {
                if (pl.CanHit(gameObject))
                {
                    TakeDamage(pl.damage);
                    pl.RegisterHit(gameObject);
                }
            }
            else
            {
                TakeDamage(1);
                Destroy(other.gameObject);
            }
        }
        // Đâm trực tiếp vào Tàu Player
        else if (other.CompareTag("Player") || other.GetComponent<PlayerCollision>() != null)
        {
            PlayerCollision playerCol = other.GetComponent<PlayerCollision>();
            if (playerCol != null)
            {
                playerCol.TakeHitFromEnemy(contactDamage);
            }

            // Tàu địch nổ luôn khi đâm cảm tử
            Die();
        }
    }
}
