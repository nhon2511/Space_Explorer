using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float delay = 0.5f; // Thời gian chạy hết 6 frame nổ

    void Start()
    {
        Destroy(gameObject, delay);
    }
}