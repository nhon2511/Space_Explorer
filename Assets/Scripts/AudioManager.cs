using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Background Music (BGM)")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float defaultBGMVolume = 0.5f;

    [Header("Laser Sound")]
    public AudioClip laserSound;
    [Range(0f, 1f)] public float laserVolume = 0.7f;
    public float laserTrimDelay = 0.76f; // Bỏ qua đoạn lặng ở đầu

    [Header("Other SFX")]
    public AudioClip explosionSound;
    [Range(0f, 1f)] public float explosionVolume = 0.8f;

    public AudioClip starSound;
    [Range(0f, 1f)] public float starVolume = 0.7f;

    public AudioClip shieldPickupSound;
    [Range(0f, 1f)] public float shieldPickupVolume = 0.8f;

    public AudioClip shieldBreakSound;
    [Range(0f, 1f)] public float shieldBreakVolume = 1.0f;

    // Master volume scales
    private float currentBGMVolume = 0.5f;
    private float currentSFXVolume = 0.8f;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // Đảm bảo là root object trước khi DontDestroyOnLoad
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        // 1. Khởi tạo BGM Source
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        // 2. Khởi tạo SFX Source
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        // 3. Đọc âm lượng từ PlayerPrefs (nếu chưa có thì lấy mặc định)
        currentBGMVolume = PlayerPrefs.GetFloat("BGM_Volume", defaultBGMVolume);
        currentSFXVolume = PlayerPrefs.GetFloat("SFX_Volume", 0.8f);

        bgmSource.volume = currentBGMVolume;

        // 4. Tự động phát BGM nếu có sẵn clip
        if (backgroundMusic != null)
        {
            PlayBGM(backgroundMusic);
        }
    }

    // --- QUẢN LÝ BGM ---
    public void PlayBGM(AudioClip clip = null)
    {
        if (clip != null)
        {
            backgroundMusic = clip;
        }

        if (backgroundMusic == null || bgmSource == null) return;

        if (bgmSource.clip == backgroundMusic && bgmSource.isPlaying) return;

        bgmSource.clip = backgroundMusic;
        bgmSource.volume = currentBGMVolume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void SetBGMVolume(float volume)
    {
        currentBGMVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = currentBGMVolume;
        }
        PlayerPrefs.SetFloat("BGM_Volume", currentBGMVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        currentSFXVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFX_Volume", currentSFXVolume);
        PlayerPrefs.Save();
    }

    public float GetBGMVolume() => currentBGMVolume;
    public float GetSFXVolume() => currentSFXVolume;

    // --- QUẢN LÝ SFX ---
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume * currentSFXVolume);
        }
    }

    public void PlaySFXCustom(AudioClip clip, float volume, float startTime)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempSFX");
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = volume * currentSFXVolume;
        aSource.time = startTime;
        aSource.pitch = Random.Range(0.95f, 1.05f);
        aSource.Play();

        float remainingDuration = (clip.length - startTime) / Mathf.Abs(aSource.pitch);
        Destroy(tempAudio, Mathf.Max(remainingDuration, 0.1f));
    }

    // --- Các hàm tiện ích gọi nhanh ---
    public void PlayLaser()
    {
        PlaySFXCustom(laserSound, laserVolume, laserTrimDelay);
    }

    public void PlayExplosion() => PlaySFX(explosionSound, explosionVolume);
    public void PlayStar() => PlaySFX(starSound, starVolume);
    public void PlayShieldPickup() => PlaySFX(shieldPickupSound, shieldPickupVolume);
    public void PlayShieldBreak() => PlaySFX(shieldBreakSound, shieldBreakVolume);
}