using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Talkable : MonoBehaviour
{
    [TextArea(1, 5)]
    public string[] lines;
    public string speakerName;
    public bool autoStartOnEnter = false;
    public bool playOnce = true;

    [SerializeField] private DialogueManager dialogueManager;

    [SerializeField] private bool isEntered;
    private bool hasPlayed;

    private void Awake()
    {
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager == null)
                Debug.LogError("Talkable: DialogueManager not found in scene.");
        }
    }

    private bool CanStartDialogue()
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("Talkable: lines 为空，无法开始对话。");
            return false;
        }
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isEntered = true;
        if (dialogueManager == null) return;

        bool canTrigger = !hasPlayed || !playOnce;
        if (!canTrigger || dialogueManager.IsOpen) return;

        if (autoStartOnEnter)
        {
            dialogueManager.HideInteractPrompt();
            if (CanStartDialogue())
            {
                dialogueManager.StartDialogue(speakerName, lines);
                if (playOnce) hasPlayed = true;
            }
        }
        else
        {
            dialogueManager.ShowInteractPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isEntered = false;
            if (dialogueManager != null)
                dialogueManager.HideInteractPrompt();
        }
    }

    private void Update()
    {
        if (!isEntered || dialogueManager == null) return;
        if (!Input.GetKeyDown(KeyCode.E) || dialogueManager.IsOpen) return;
        if (hasPlayed && playOnce) return;
        if (!CanStartDialogue()) return;

        dialogueManager.HideInteractPrompt();
        dialogueManager.StartDialogue(speakerName, lines);
        if (playOnce) hasPlayed = true;
    }
}
