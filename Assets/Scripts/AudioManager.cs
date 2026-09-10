using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Laser Sound")]
    public AudioClip laserSound;
    [Range(0f, 1f)] public float laserVolume = 0.7f;
    public float laserTrimDelay = 0.76f; // Bỏ qua đúng 0.76 giây đầu

    [Header("Other SFX")]
    public AudioClip explosionSound;
    [Range(0f, 1f)] public float explosionVolume = 0.8f;

    public AudioClip starSound;
    [Range(0f, 1f)] public float starVolume = 0.7f;

    public AudioClip shieldPickupSound;
    [Range(0f, 1f)] public float shieldPickupVolume = 0.8f;

    public AudioClip shieldBreakSound;
    [Range(0f, 1f)] public float shieldBreakVolume = 1.0f;

    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Hàm phát âm thanh thông thường (không bị trễ)
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    // Hàm riêng cho âm thanh bị trễ ở đầu:
    // Tạo 1 audio source tạm thời, tua ngay đến mốc startTime rồi tự hủy sau khi phát xong
    public void PlaySFXCustom(AudioClip clip, float volume, float startTime)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempSFX");
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = volume;
        aSource.time = startTime; // Nhảy cóc qua đoạn lặng (0.76s)
        aSource.pitch = Random.Range(0.95f, 1.05f); // Âm sắc ngẫu nhiên nhẹ cho hay
        aSource.Play();

        // Tự động xóa đối tượng tạm thời sau khi clip chạy hết
        float remainingDuration = (clip.length - startTime) / Mathf.Abs(aSource.pitch);
        Destroy(tempAudio, Mathf.Max(remainingDuration, 0.1f));
    }

    // --- Các hàm gọi nhanh ---
    public void PlayLaser()
    {
        PlaySFXCustom(laserSound, laserVolume, laserTrimDelay);
    }

    public void PlayExplosion() => PlaySFX(explosionSound, explosionVolume);
    public void PlayStar() => PlaySFX(starSound, starVolume);
    public void PlayShieldPickup() => PlaySFX(shieldPickupSound, shieldPickupVolume);
    public void PlayShieldBreak() => PlaySFX(shieldBreakSound, shieldBreakVolume);
}