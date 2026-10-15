using System.Collections.Generic;
using UnityEngine;

public class OrbitalDrone : MonoBehaviour
{
    [Header("Orbit Configuration")]
    public Transform targetTransform;
    public float orbitRadius = 1.75f;
    public float orbitSpeed = 160f;
    public float startingAngle = 0f;

    [Header("Combat Properties")]
    public int contactDamage = 2;
    public float damageTickRate = 0.35f;

    private float currentAngle;
    private bool isAngleInitialized = false;
    private Dictionary<int, float> hitCooldowns = new Dictionary<int, float>();

    void Start()
    {
        if (!isAngleInitialized)
        {
            currentAngle = startingAngle;
        }

        if (targetTransform == null && PlayerController.Instance != null)
        {
            targetTransform = PlayerController.Instance.transform;
        }
    }

    void Update()
    {
        if (targetTransform == null)
        {
            if (PlayerController.Instance != null)
                targetTransform = PlayerController.Instance.transform;
            else
                return;
        }

        // 1. Bay quỹ đạo tròn quanh Tàu
        currentAngle += orbitSpeed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
        transform.position = targetTransform.position + offset;

        // 2. Tự xoay quanh tâm bản thân tạo cảm giác chuyển động
        transform.Rotate(0f, 0f, -240f * Time.deltaTime);

        // 3. Làm sạch bộ đếm cooldown
        CleanCooldowns();
    }

    public void SetOrbitParams(Transform target, float radius, float speed, float initialAngle)
    {
        targetTransform = target;
        orbitRadius = radius;
        orbitSpeed = speed;
        startingAngle = initialAngle;
        currentAngle = initialAngle;
        isAngleInitialized = true;

        // Cập nhật ngay vị trí để không bị trễ frame đầu
        if (target != null)
        {
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
            transform.position = target.position + offset;
        }
    }

    private void CleanCooldowns()
    {
        List<int> expiredKeys = null;
        float now = Time.time;
        foreach (var kvp in hitCooldowns)
        {
            if (now >= kvp.Value)
            {
                if (expiredKeys == null) expiredKeys = new List<int>();
                expiredKeys.Add(kvp.Key);
            }
        }

        if (expiredKeys != null)
        {
            for (int i = 0; i < expiredKeys.Count; i++)
            {
                hitCooldowns.Remove(expiredKeys[i]);
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryHit(other.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other.gameObject);
    }

    private void TryHit(GameObject obj)
    {
        int id = obj.GetInstanceID();
        if (hitCooldowns.ContainsKey(id)) return;

        bool hitSuccess = false;

        // Trúng tàu địch hoặc Boss
        EnemyBase enemy = obj.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(contactDamage);
            hitSuccess = true;
        }
        else
        {
            // Trúng thiên thạch
            Asteroid asteroid = obj.GetComponent<Asteroid>();
            if (asteroid != null)
            {
                // Bắn nổ thiên thạch bằng cách mô phỏng va chạm
                if (GameManager.Instance != null)
                    GameManager.Instance.AddScore(asteroid.scoreValue, asteroid.transform.position);

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayExplosion();

                if (asteroid.explosionPrefab != null)
                    Instantiate(asteroid.explosionPrefab, asteroid.transform.position, Quaternion.identity);

                Destroy(asteroid.gameObject);
                hitSuccess = true;
            }
        }

        if (hitSuccess)
        {
            hitCooldowns[id] = Time.time + damageTickRate;

            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(0.08f, 0.1f);
        }
    }
}
