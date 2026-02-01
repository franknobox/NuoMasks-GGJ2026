using UnityEngine;

/// <summary>
/// 跨场景持续播放同一段 BGM：切场景不中断、不重头播、不叠加。
/// 在 Inspector 中为 AudioSource 指定 Audio Clip；建议关闭 Play On Awake，由本脚本在 Awake 中统一 Play。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    private static BGMManager _instance;
    private AudioSource _audioSource;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) return;

        _audioSource.loop = true;
        if (!_audioSource.isPlaying && _audioSource.clip != null)
            _audioSource.Play();
    }
}
