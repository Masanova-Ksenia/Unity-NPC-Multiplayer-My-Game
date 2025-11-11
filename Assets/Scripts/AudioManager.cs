using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource; // Для фоновой музыки
    public AudioSource sfxSource;   // Для звуковых эффектов

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip coinCollectSound;
    public AudioClip deathSound;
    public AudioClip winSound;

    void Awake()
    {
        // Singleton паттерн
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Запустить фоновую музыку
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.volume = 0.5f;
            musicSource.Play();
        }
    }

    public void PlayCoinSound()
    {
        if (coinCollectSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(coinCollectSound, 0.7f);
        }
    }

    public void PlayDeathSound()
    {
        if (deathSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(deathSound, 1f);
        }
    }

    public void PlayWinSound()
    {
        if (winSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(winSound, 1f);
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }

    public void RestartMusic()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.Stop();
            musicSource.Play();
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
    }
}