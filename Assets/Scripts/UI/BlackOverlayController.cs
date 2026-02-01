using UnityEngine;

/// <summary>
/// Controls a full-screen black overlay (e.g. CanvasGroup on an Image).
/// Assign a GameObject with CanvasGroup + Image (black) in the inspector.
/// </summary>
public class BlackOverlayController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject root;

    public void Show(float fadeDuration)
    {
        if (root != null) root.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    public void Hide(float fadeDuration)
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (root != null) root.SetActive(false);
    }
}
