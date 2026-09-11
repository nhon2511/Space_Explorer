using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 initialPosition;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    // Hàm gọi rung màn hình với thời gian (duration) và độ mạnh (magnitude)
    public void Shake(float duration = 0.15f, float magnitude = 0.2f)
    {
        StopAllCoroutines(); // Ngắt rung cũ nếu có rung mới đè lên
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float xOffset = Random.Range(-1f, 1f) * magnitude;
            float yOffset = Random.Range(-1f, 1f) * magnitude;

            // Giữ nguyên trục Z để không lỗi hiển thị Camera 2D
            transform.localPosition = new Vector3(initialPosition.x + xOffset, initialPosition.y + yOffset, initialPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Trả Camera về đúng vị trí ban đầu
        transform.localPosition = initialPosition;
    }
}