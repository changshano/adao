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
    [SerializeField] private AudioSource movementSource; // 专门用于移动音效的音频源

    [Header("背景音乐")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("战斗背景音乐")] // 新增的标题
    [SerializeField] private AudioClip battleBackgroundMusic; // 新增的战斗背景音乐字段

    [Header("角色音效")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip deathSound;

    [Header("移动音效")]
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private bool loopMoveSound = true; // 是否循环播放移动音效
    [SerializeField] private float moveSoundDelay = 0.5f; // 移动音效播放间隔

    [Header("其他音效列表")]
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
    private Coroutine moveSoundCoroutine;
    private bool isMoving = false;

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
        if (movementSource == null) movementSource = gameObject.AddComponent<AudioSource>();

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
        // 初始化特定角色音效到字典
        if (attackSound != null && !soundEffectDict.ContainsKey("Attack"))
            soundEffectDict["Attack"] = attackSound;

        if (hurtSound != null && !soundEffectDict.ContainsKey("Hurt"))
            soundEffectDict["Hurt"] = hurtSound;

        if (jumpSound != null && !soundEffectDict.ContainsKey("Jump"))
            soundEffectDict["Jump"] = jumpSound;

        if (deathSound != null && !soundEffectDict.ContainsKey("Death"))
            soundEffectDict["Death"] = deathSound;

        if (moveSound != null && !soundEffectDict.ContainsKey("Move"))
            soundEffectDict["Move"] = moveSound;

        // 初始化其他音频剪辑字典
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

        // 移动音效源设置
        movementSource.loop = false;
        movementSource.volume = sfxVolume;
        movementSource.playOnAwake = false;
        movementSource.outputAudioMixerGroup = audioMixer?.FindMatchingGroups("SFX")?[0];
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

    // 新增：播放战斗背景音乐（带淡入淡出效果）
    public void PlayBattleMusic(bool fadeIn = false)
    {
        if (battleBackgroundMusic != null)
        {
            PlayBackgroundMusic(battleBackgroundMusic, fadeIn);
        }
        else
        {
            Debug.LogWarning("战斗背景音乐未分配！");
        }
    }

    // 新增：切换回普通背景音乐（带淡入淡出效果）
    public void PlayNormalBackgroundMusic(bool fadeIn = false)
    {
        if (backgroundMusic != null)
        {
            PlayBackgroundMusic(backgroundMusic, fadeIn);
        }
        else
        {
            Debug.LogWarning("普通背景音乐未分配！");
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

    // ===== 角色音效专用方法 =====

    public void PlayAttackSound(float volume = 1f, float pitch = 1f)
    {
        if (!sfxEnabled || attackSound == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(attackSound, volume);
    }

    public void PlayHurtSound(float volume = 1f, float pitch = 1f)
    {
        if (!sfxEnabled || hurtSound == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(hurtSound, volume);
    }

    public void PlayJumpSound(float volume = 1f, float pitch = 1f)
    {
        if (!sfxEnabled || jumpSound == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(jumpSound, volume);
    }

    public void PlayDeathSound(float volume = 1f, float pitch = 1f)
    {
        if (!sfxEnabled || deathSound == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(deathSound, volume);
    }

    // 开始播放移动音效
    public void StartMoveSound(bool loop = true, float delay = 0.5f)
    {
        if (!sfxEnabled || moveSound == null || isMoving) return;

        isMoving = true;

        if (loop && loopMoveSound)
        {
            // 循环播放移动音效
            moveSoundCoroutine = StartCoroutine(PlayMoveSoundLoop(delay));
        }
        else
        {
            // 播放一次移动音效
            movementSource.PlayOneShot(moveSound, sfxVolume);
        }
    }

    // 停止播放移动音效
    public void StopMoveSound()
    {
        if (!isMoving) return;

        isMoving = false;

        if (moveSoundCoroutine != null)
        {
            StopCoroutine(moveSoundCoroutine);
            moveSoundCoroutine = null;
        }

        if (movementSource.isPlaying)
        {
            movementSource.Stop();
        }
    }

    private IEnumerator PlayMoveSoundLoop(float delay)
    {
        while (isMoving)
        {
            movementSource.PlayOneShot(moveSound, sfxVolume);
            yield return new WaitForSeconds(delay);
        }
    }

    // ===== 其他原有方法 =====

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
        movementSource.volume = sfxVolume;
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

    // 获取特定音效
    public AudioClip GetAttackSound() => attackSound;
    public AudioClip GetHurtSound() => hurtSound;
    public AudioClip GetJumpSound() => jumpSound;
    public AudioClip GetDeathSound() => deathSound;
    public AudioClip GetMoveSound() => moveSound;

    // 新增：获取战斗背景音乐
    public AudioClip GetBattleBackgroundMusic() => battleBackgroundMusic;

    // 设置特定音效
    public void SetAttackSound(AudioClip clip) => attackSound = clip;
    public void SetHurtSound(AudioClip clip) => hurtSound = clip;
    public void SetJumpSound(AudioClip clip) => jumpSound = clip;
    public void SetDeathSound(AudioClip clip) => deathSound = clip;
    public void SetMoveSound(AudioClip clip) => moveSound = clip;

    // 新增：设置战斗背景音乐
    public void SetBattleBackgroundMusic(AudioClip clip) => battleBackgroundMusic = clip;



}