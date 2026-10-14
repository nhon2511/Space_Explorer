using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.8f;
    public float fadeSpeed = 1.5f;
    public float lifeTime = 0.8f;

    private TextMeshPro tmp;
    private Color textColor;
    private float timer = 0f;

    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            textColor = tmp.color;
        }
    }

    public void Setup(string text, Color color, float fontSize = 5f)
    {
        if (tmp == null) tmp = GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
            tmp.fontSize = fontSize;
            textColor = color;
        }
    }

    void Update()
    {
        // 1. Bay nhẹ lên trên
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 2. Mờ dần alpha theo thời gian
        timer += Time.deltaTime;
        if (tmp != null && timer > (lifeTime * 0.4f))
        {
            textColor.a -= fadeSpeed * Time.deltaTime;
            tmp.color = textColor;
        }

        // 3. Tự hủy
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
