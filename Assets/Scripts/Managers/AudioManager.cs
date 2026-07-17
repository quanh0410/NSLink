using UnityEngine;
using UnityEngine.Audio;

namespace PolarBond.Managers
{
    [System.Serializable]
    public class SoundSettings
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource bgmSource;
        private AudioSource sfxSource;

        [Header("Audio Mixer (Optional)")]
        public AudioMixer mainMixer;

        [Header("Background Music")]
        public SoundSettings bgm = new SoundSettings() { volume = 0.5f };

        [Header("Sound Effects")]
        public SoundSettings linkSound = new SoundSettings() { volume = 1f };
        public SoundSettings moveSound = new SoundSettings() { volume = 0.4f };
        public SoundSettings targetSound = new SoundSettings() { volume = 1f };
        public SoundSettings winSound = new SoundSettings() { volume = 1f };
        public SoundSettings errorSound = new SoundSettings() { volume = 0.7f };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                // Thử tìm Prefab cấu hình sẵn trong Resources
                GameObject prefab = Resources.Load<GameObject>("AudioManagerPrefab");
                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab);
                    go.name = "AudioManager";
                    Instance = go.GetComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
                else
                {
                    // Fallback tự động tạo nếu không có Prefab
                    GameObject go = new GameObject("AudioManager");
                    Instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            bgmSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();

            SetupMixerAndClips();
        }

        private void Start()
        {
            PlayBGM();
        }

        private void SetupMixerAndClips()
        {
            // Nếu không được gán sẵn qua Inspector, thử tự động tìm trong Resources
            if (mainMixer == null) mainMixer = Resources.Load<AudioMixer>("MainMixer");
            if (mainMixer != null)
            {
                var bgmGroup = mainMixer.FindMatchingGroups("BGM");
                if (bgmGroup.Length > 0) bgmSource.outputAudioMixerGroup = bgmGroup[0];
                
                var sfxGroup = mainMixer.FindMatchingGroups("SFX");
                if (sfxGroup.Length > 0) sfxSource.outputAudioMixerGroup = sfxGroup[0];
            }

            // Tự động load clip nếu clip trống (Trường hợp chưa dùng Prefab)
            if (bgm.clip == null) bgm.clip = Resources.Load<AudioClip>("SFX/backgroundmusic");
            if (linkSound.clip == null) linkSound.clip = Resources.Load<AudioClip>("SFX/discord-notification");
            if (moveSound.clip == null) moveSound.clip = Resources.Load<AudioClip>("SFX/dry-fart");
            if (targetSound.clip == null) targetSound.clip = Resources.Load<AudioClip>("SFX/iphone-charge-sound");
            if (winSound.clip == null) winSound.clip = Resources.Load<AudioClip>("SFX/win");
            if (errorSound.clip == null) errorSound.clip = Resources.Load<AudioClip>("SFX/Error 2");
        }

        public void PlayBGM()
        {
            if (bgmSource != null && bgm.clip != null)
            {
                bgmSource.clip = bgm.clip;
                // Áp dụng volume tổng (Master BGM Volume lưu trong PlayerPrefs)
                float bgmMasterVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
                bgmSource.volume = bgm.volume * bgmMasterVolume;
                bgmSource.pitch = bgm.pitch;
                bgmSource.loop = true;
                bgmSource.Play();
            }
        }

        public void SetBGMVolume(float volume)
        {
            if (bgmSource != null && bgm.clip != null)
            {
                bgmSource.volume = bgm.volume * volume;
            }
        }

        public void PlayLinkSound() { PlaySFX(linkSound); }
        public void PlayMoveSound() { PlaySFX(moveSound); }
        public void PlayTargetSound() { PlaySFX(targetSound); }
        public void PlayWinSound() { PlaySFX(winSound); }
        public void PlayErrorSound() { PlaySFX(errorSound); }

        private void PlaySFX(SoundSettings settings)
        {
            if (sfxSource != null && settings != null && settings.clip != null)
            {
                sfxSource.pitch = settings.pitch;
                // Áp dụng volume tổng (Master SFX Volume lưu trong PlayerPrefs)
                float sfxMasterVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxSource.PlayOneShot(settings.clip, settings.volume * sfxMasterVolume);
            }
        }
    }
}
