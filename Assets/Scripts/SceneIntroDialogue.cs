// Inspector：将 Canvas 下 BlackOverlay(Image) 上的 CanvasGroup 拖到 blackOverlayCg。
// 层级上 BlackOverlay 需在 Dialogue Panel 下面，这样黑幕盖住世界但不遮住对话 UI。
using System.Collections;
using UnityEngine;

public class SceneIntroDialogue : MonoBehaviour
{
    [TextArea(1, 5)]
    public string[] introLines;
    public string speakerName;
    public bool playOncePerRun = true;

    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private CanvasGroup blackOverlayCg;
    [SerializeField] private float fadeDuration = 0.3f;
    private static bool played;

    private void Start()
    {
        StartCoroutine(PlayIntroNextFrame());
    }

    private IEnumerator PlayIntroNextFrame()
    {
        yield return null;

        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogError("SceneIntroDialogue: DialogueManager not found.");
            yield break;
        }
        if (playOncePerRun && played) yield break;
        if (introLines == null || introLines.Length == 0) yield break;
        if (dialogueManager.IsOpen) yield break;

        bool overlayShown = false;
        if (blackOverlayCg != null)
        {
            blackOverlayCg.blocksRaycasts = true;
            yield return FadeCanvasGroup(blackOverlayCg, 0f, 1f, fadeDuration);
            overlayShown = true;
        }

        dialogueManager.StartDialogue(speakerName, introLines);
        if (playOncePerRun) played = true;

        yield return new WaitUntil(() => dialogueManager.IsOpen);
        yield return new WaitUntil(() => !dialogueManager.IsOpen);

        if (overlayShown && blackOverlayCg != null)
        {
            yield return FadeCanvasGroup(blackOverlayCg, 1f, 0f, fadeDuration);
            blackOverlayCg.blocksRaycasts = false;
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;
        if (duration <= 0f)
        {
            cg.alpha = to;
            yield break;
        }
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        cg.alpha = to;
    }
}
