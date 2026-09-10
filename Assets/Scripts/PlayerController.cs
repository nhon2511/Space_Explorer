using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public GameObject laserPrefab;
    public Transform firePoint;
    public float laserSpeed = 12f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Di chuyển bằng 4 phím mũi tên hoặc A-D-W-S
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Nhấn phím Space để bắn tia laser
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }

    void Shoot()
    {
        if (laserPrefab != null && firePoint != null)
        {
            GameObject laser = Instantiate(laserPrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D laserRb = laser.GetComponent<Rigidbody2D>();
            if (laserRb == null) laserRb = laser.AddComponent<Rigidbody2D>();
            laserRb.gravityScale = 0;
            laserRb.linearVelocity = Vector2.up * laserSpeed;
            Destroy(laser, 2.5f); // Tự hủy viên đạn sau 2.5 giây

            // Phát âm thanh tiếng súng Laser
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLaser();
            }
        }
    }
}