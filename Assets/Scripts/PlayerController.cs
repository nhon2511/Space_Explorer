using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;

    [Header("Weapon Settings")]
    public GameObject laserPrefab;
    public Transform firePoint;
    public float laserSpeed = 12f;

    [Header("VFX Settings")]
    public Animator thrusterAnimator; // Kéo đối tượng con ThrusterVisual vào đây

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. Nhận phím di chuyển (mũi tên hoặc WASD)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 2. Cập nhật hoạt ảnh lửa đuôi tàu
        if (thrusterAnimator != null)
        {
            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            thrusterAnimator.SetBool("isMoving", isMoving);
        }

        // 3. Nhấn phím Space để bắn tia laser
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