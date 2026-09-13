using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("Tốc độ trôi của nền vũ trụ")]
    public float scrollSpeed = 0.15f;

    private Material bgMaterial;
    private Vector2 offset = Vector2.zero;

    void Start()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            bgMaterial = mr.material;
        }
    }

    void Update()
    {
        if (bgMaterial != null)
        {
            // Tăng tọa độ Y của Offset theo thời gian để cuộn nền xuống dưới
            offset.y += scrollSpeed * Time.deltaTime;
            bgMaterial.mainTextureOffset = offset;
        }
    }
}