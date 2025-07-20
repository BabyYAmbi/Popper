using UnityEngine;

namespace PopperBurst
{
    public enum SFXClips
    {
        Pop,
        LevelClear,
        LevelFail
    }

    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;

        [Header("Audio Clips")]
        public AudioClip backgroundMusic;
        public AudioClip[] sfxClips;

        [Header("Volume")]
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        [Range(0f, 1f)] public float sfxVolume = 1f;

        public static AudioManager Instance;

        void Awake()
        {
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
            if (musicSource != null)
                musicSource.volume = musicVolume;
            if (sfxSource != null)
                sfxSource.volume = sfxVolume;
        }

        public void PlayMusic()
        {
            if (musicSource != null && backgroundMusic != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.volume = musicVolume;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            if (musicSource != null)
                musicSource.Stop();
        }

        public void PlaySFX(SFXClips sfxClip)
        {
            int index = (int)sfxClip;
            if (sfxSource != null && index >= 0 && index < sfxClips.Length && sfxClips[index] != null)
            {
                sfxSource.PlayOneShot(sfxClips[index], sfxVolume);
            }
        }
    }
}