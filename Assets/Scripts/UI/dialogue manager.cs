using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueBox; // Display or Hide
    public Text dialogueText, nameText;

    [TextArea(1, 3)]
    public string[] dialogueLines;
    [SerializeField] private int currentLine;

    [Header("交互与玩家")]
    [SerializeField] private GameObject interactPromptRoot;
    [SerializeField] private MonoBehaviour playerMovementRef;

    private MonoBehaviour _cachedMovement;

    public bool IsOpen => dialogueBox != null && dialogueBox.activeSelf;

    private void Start()
    {
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    public void ShowInteractPrompt()
    {
        if (IsOpen) return;
        if (interactPromptRoot != null)
            interactPromptRoot.SetActive(true);
    }

    public void HideInteractPrompt()
    {
        if (interactPromptRoot != null)
            interactPromptRoot.SetActive(false);
    }

    private void ApplyMovementEnabled(bool enabled)
    {
        MonoBehaviour m = GetPlayerMovement();
        if (m == null) return;
        if (m is PlayerGo pg)
            pg.SetMovementEnabled(enabled);
        else if (m is Player p)
            p.SetMovementEnabled(enabled);
    }

    private MonoBehaviour GetPlayerMovement()
    {
        if (playerMovementRef != null)
        {
            _cachedMovement = playerMovementRef;
            return playerMovementRef;
        }
        if (_cachedMovement != null) return _cachedMovement;
        PlayerGo pg = FindObjectOfType<PlayerGo>();
        if (pg != null) { _cachedMovement = pg; return pg; }
        Player p = FindObjectOfType<Player>();
        if (p != null) { _cachedMovement = p; return p; }
        Debug.LogError("DialogueManager: 未找到玩家移动脚本（PlayerGo 或 Player）。");
        return null;
    }

    public void StartDialogue(string speakerName, string[] lines)
    {
        currentLine = 0;
        if (nameText != null)
            nameText.text = speakerName ?? string.Empty;
        dialogueLines = lines != null ? lines : System.Array.Empty<string>();
        HideInteractPrompt();
        ApplyMovementEnabled(false);
        if (dialogueBox != null)
            dialogueBox.SetActive(true);
        if (dialogueText != null && dialogueLines.Length > 0)
            dialogueText.text = dialogueLines[0];
    }

    public void Advance()
    {
        currentLine++;
        if (dialogueLines != null && currentLine < dialogueLines.Length)
        {
            if (dialogueText != null)
                dialogueText.text = dialogueLines[currentLine];
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        ApplyMovementEnabled(true);
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    private void Update()
    {
        if (IsOpen && (Input.GetMouseButtonUp(0) || Input.GetKeyDown(KeyCode.E)))
            Advance();
    }
}
