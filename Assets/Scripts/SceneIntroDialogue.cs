using System.Collections;
using UnityEngine;

public class SceneIntroDialogue : MonoBehaviour
{
    [TextArea(1, 5)]
    public string[] introLines;
    public string speakerName;
    public bool playOncePerRun = true;

    [SerializeField] private DialogueManager dialogueManager;
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

        dialogueManager.StartDialogue(speakerName, introLines);
        if (playOncePerRun) played = true;
    }
}
