using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject asteroidPrefab;
    public GameObject starPrefab;

    [Header("Spawn Rate")]
    [Tooltip("Khoảng cách thời gian sinh vật thể (càng nhỏ sinh càng nhanh)")]
    public float spawnInterval = 0.8f; // Giảm xuống 0.8s để xuất hiện liên tục

    [Header("Spawn Position Boundaries")]
    public float minX = -7.5f;
    public float maxX = 7.5f;
    public float spawnY = 6f; // Vị trí ở phía trên mép trên của camera

    void Start()
    {
        // Bắt đầu sau 0.5s và lặp lại liên tục sau mỗi spawnInterval giây
        InvokeRepeating(nameof(SpawnObject), 0.5f, spawnInterval);
    }

    void SpawnObject()
    {
        // Vị trí sinh: nằm ngang ngẫu nhiên ở phía trên đỉnh màn hình (Y = 6)
        Vector2 spawnPos = new Vector2(Random.Range(minX, maxX), spawnY);

        // Tỉ lệ: 75% sinh Thiên thạch (nhiều hơn), 25% sinh Ngôi sao
        if (Random.value < 0.75f)
        {
            if (asteroidPrefab != null)
                Instantiate(asteroidPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // Ngôi sao có thể sinh rải rác ngẫu nhiên ngay trong màn hình để người chơi dễ nhặt
            Vector2 starPos = new Vector2(Random.Range(minX, maxX), Random.Range(-3.5f, 4f));
            if (starPrefab != null)
                Instantiate(starPrefab, starPos, Quaternion.identity);
        }
    }
}