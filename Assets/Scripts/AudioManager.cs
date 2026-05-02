using UnityEngine;

/// <summary>
/// 音频管理：音效、单词发音
/// 无外部音频文件时自动生成占位音效（正弦波），可后续替换为真实音频
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音量")]
    [Range(0, 1)] public float sfxVolume = 0.5f;
    [Range(0, 1)] public float voiceVolume = 0.5f;

    private AudioSource sfxSource;   // 音效
    private AudioSource voiceSource; // 单词发音

    private AudioClip correctClip;
    private AudioClip wrongClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化音频源并生成占位音效
    /// </summary>
    private void InitAudio()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = sfxVolume;

        voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.volume = voiceVolume;

        GeneratePlaceholderAudio();
    }

    /// <summary>
    /// 生成占位音效
    /// </summary>
    private void GeneratePlaceholderAudio()
    {
        correctClip = CreateToneClip("CorrectSFX", 880, 0.25f, 0.4f); // A5 清亮音
        wrongClip = CreateToneClip("WrongSFX", 220, 0.4f, 0.3f);      // A3 低沉音
    }

    /// <summary>
    /// 生成单音调 AudioClip
    /// </summary>
    private static AudioClip CreateToneClip(string name, float frequency, float duration, float amplitude)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Clamp01(t * 40f) * Mathf.Clamp01((duration - t) * 40f);
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * amplitude * envelope;
        }

        AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// 根据单词生成一个占位发音音调（根据单词长度和首字母决定音高，让玩家感知到音频反馈）
    /// </summary>
    private static AudioClip CreateWordToneClip(string word)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        float duration = Mathf.Clamp(word.Length * 0.08f, 0.3f, 1f);
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples];

        // 用单词长度和首字母决定音高，不同单词发出不同音调
        float baseFreq = 300 + (word.Length % 7) * 60 + (word[0] % 5) * 30;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Clamp01(t * 20f) * Mathf.Clamp01((duration - t) * 10f);
            // 主音 + 轻微颤音模拟发音感
            float freq = baseFreq + Mathf.Sin(t * 6f) * 15f;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.3f * envelope;
        }

        AudioClip clip = AudioClip.Create($"Voice_{word}", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// 设置真实音效剪辑（替换占位音效）
    /// </summary>
    public void SetCorrectClip(AudioClip clip) { if (clip != null) correctClip = clip; }
    public void SetWrongClip(AudioClip clip) { if (clip != null) wrongClip = clip; }

    /// <summary>
    /// 播放答对音效
    /// </summary>
    public void PlayCorrectSFX()
    {
        if (sfxSource != null && correctClip != null)
            sfxSource.PlayOneShot(correctClip, sfxVolume);
    }

    /// <summary>
    /// 播放答错音效
    /// </summary>
    public void PlayWrongSFX()
    {
        if (sfxSource != null && wrongClip != null)
            sfxSource.PlayOneShot(wrongClip, sfxVolume);
    }

    /// <summary>
    /// 播放单词发音
    /// 优先加载 Resources/Voice/{word}.mp3，没有则生成占位音调
    /// </summary>
    public void PlayWordVoice(string word)
    {
        if (voiceSource == null || string.IsNullOrEmpty(word)) return;

        // 优先从 Resources/Voice/ 加载外部音频文件
        AudioClip clip = Resources.Load<AudioClip>($"Voice/{word}");
        if (clip == null)
        {
            // 没有外部文件时，生成占位音调
            clip = CreateWordToneClip(word);
        }

        voiceSource.PlayOneShot(clip, voiceVolume);
    }
}
