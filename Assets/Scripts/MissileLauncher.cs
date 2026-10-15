using System.Collections;
using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    [Header("Missile Launcher Settings")]
    public GameObject missilePrefab;
    public float fireInterval = 3.5f;
    public int missilesPerVolley = 1;
    public bool isUnlocked = false;

    private float timer = 0f;

    void Start()
    {
        if (missilePrefab == null)
        {
            missilePrefab = Resources.Load<GameObject>("HomingMissile");
        }
    }

    void Update()
    {
        if (!isUnlocked) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = fireInterval;
            StartCoroutine(FireVolleyRoutine());
        }
    }

    private IEnumerator FireVolleyRoutine()
    {
        if (missilePrefab == null)
            missilePrefab = Resources.Load<GameObject>("HomingMissile");

        if (missilePrefab == null) yield break;

        switch (missilesPerVolley)
        {
            case 1:
                // Cấp 1: 1 quả bắn từ mũi tàu
                Instantiate(missilePrefab, transform.position + new Vector3(0f, 0.2f, 0f), Quaternion.identity);
                break;

            case 2:
                // Cấp 2: 2 quả bắn đồng thời từ 2 bên cánh
                Instantiate(missilePrefab, transform.position + new Vector3(-0.45f, 0.05f, 0f), Quaternion.identity);
                Instantiate(missilePrefab, transform.position + new Vector3(0.45f, 0.05f, 0f), Quaternion.identity);
                break;

            case 3:
                // Cấp 3: 3 quả bắn chùm (trái, giữa, phải)
                Instantiate(missilePrefab, transform.position + new Vector3(-0.5f, 0f, 0f), Quaternion.identity);
                Instantiate(missilePrefab, transform.position + new Vector3(0f, 0.25f, 0f), Quaternion.identity);
                Instantiate(missilePrefab, transform.position + new Vector3(0.5f, 0f, 0f), Quaternion.identity);
                break;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLaser();
        }

        yield return null;
    }

    public void UpgradeLauncher(int level)
    {
        isUnlocked = true;
        if (missilePrefab == null)
        {
            missilePrefab = Resources.Load<GameObject>("HomingMissile");
        }

        switch (level)
        {
            case 1:
                fireInterval = 2.8f;
                missilesPerVolley = 1; // 1 quả
                break;
            case 2:
                fireInterval = 2.2f;
                missilesPerVolley = 2; // 2 quả
                break;
            case 3:
                fireInterval = 1.6f;
                missilesPerVolley = 3; // 3 quả
                break;
        }

        // Bắn ngay 1 loạt tên lửa thị uy ngay khi người chơi vừa chọn nâng cấp
        timer = fireInterval;
        StartCoroutine(FireVolleyRoutine());

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpawnFloatingText($"[TÊN LỬA: {missilesPerVolley} QUẢ/ĐỢT]", transform.position, Color.yellow, 6f);
        }
    }
}
