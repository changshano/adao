using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音频混合器")]
    public AudioMixer audioMixer;

    [Header("音频源")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;

    [Header("音频剪辑")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private List<AudioClip> soundEffects = new List<AudioClip>();

    [Header("音量设置")]
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;
    [Range(0f, 1f)]
    public float uiVolume = 1f;

    [Header("音频设置")]
    public bool musicEnabled = true;
    public bool sfxEnabled = true;
    public float fadeDuration = 1f;

    private Dictionary<string, AudioClip> soundEffectDict = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 如果需要，自动获取音频源
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (uiSource == null) uiSource = gameObject.AddComponent<AudioSource>();

        // 设置音频源属性
        SetupAudioSources();

        // 播放背景音乐
        if (backgroundMusic != null && musicEnabled)
        {
            PlayBackgroundMusic(backgroundMusic, true);
        }
    }

    private void InitializeAudioManager()
    {
        // 初始化音频剪辑字典
        foreach (var clip in soundEffects)
        {
            if (clip != null && !soundEffectDict.ContainsKey(clip.name))
            {
                soundEffectDict[clip.name] = clip;
            }
        }

        Debug.Log("AudioManager 初始化完成");
    }

    private void SetupAudioSources()
    {
        // 音乐源设置
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;
        musicSource.outputAudioMixerGroup = audioMixer?.FindMatchingGroups("Music")?[0];

        // 音效源设置
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
        sfxSource.playOnAwake = false;
        sfxSource.outputAudioMixerGroup = audioMixer?.FindMatchingGroups("SFX")?[0];

        // UI音效源设置
        uiSource.loop = false;
        uiSource.volume = uiVolume;
        uiSource.playOnAwake = false;
        uiSource.outputAudioMixerGroup = audioMixer?.FindMatchingGroups("UI")?[0];
    }

    // 播放背景音乐
    public void PlayBackgroundMusic(AudioClip clip, bool fadeIn = false)
    {
        if (!musicEnabled || clip == null) return;

        if (fadeIn && musicSource.isPlaying)
        {
            StartCoroutine(FadeMusic(clip));
        }
        else
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    // 播放音效
    public void PlaySFX(string clipName, float volume = 1f, float pitch = 1f)
    {
        if (!sfxEnabled || !soundEffectDict.ContainsKey(clipName)) return;

        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(soundEffectDict[clipName], volume);
    }

    // 播放UI音效
    public void PlayUISound(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        uiSource.PlayOneShot(clip, volume);
    }

    // 播放音效（通过AudioClip）
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (!sfxEnabled || clip == null) return;

        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, volume);
    }

    // 停止背景音乐
    public void StopMusic(bool fadeOut = false)
    {
        if (fadeOut)
        {
            StartCoroutine(FadeOutMusic());
        }
        else
        {
            musicSource.Stop();
        }
    }

    // 暂停背景音乐
    public void PauseMusic()
    {
        musicSource.Pause();
    }

    // 继续播放背景音乐
    public void ResumeMusic()
    {
        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    // 淡入淡出音乐
    private IEnumerator FadeMusic(AudioClip newClip)
    {
        yield return StartCoroutine(FadeOutMusic());

        musicSource.clip = newClip;
        musicSource.Play();

        yield return StartCoroutine(FadeInMusic());
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;
    }

    private IEnumerator FadeInMusic()
    {
        float targetVolume = musicVolume;
        musicSource.volume = 0;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, targetVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    // 设置音量
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        uiSource.volume = uiVolume;
    }

    // 切换音乐开关
    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;

        if (musicEnabled && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
        else if (!musicEnabled && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    // 切换音效开关
    public void ToggleSFX()
    {
        sfxEnabled = !sfxEnabled;
    }

    // 添加音效到字典
    public void AddSoundEffect(AudioClip clip)
    {
        if (clip != null && !soundEffectDict.ContainsKey(clip.name))
        {
            soundEffectDict.Add(clip.name, clip);
        }
    }

    // 从字典移除音效
    public void RemoveSoundEffect(string clipName)
    {
        if (soundEffectDict.ContainsKey(clipName))
        {
            soundEffectDict.Remove(clipName);
        }
    }

    // 获取音效列表
    public List<string> GetAvailableSoundEffects()
    {
        return new List<string>(soundEffectDict.Keys);
    }
}