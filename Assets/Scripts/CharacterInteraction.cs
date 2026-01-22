using UnityEngine;
using Yarn.Unity;

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterInteraction : MonoBehaviour
{
    [Header("Yarn Spinner Settings")]
    [SerializeField] private string dialogueNodeName = "Start";

    private DialogueRunner dialogueRunner;

    private void Start()
    {
        dialogueRunner = GetComponent<DialogueRunner>();
    }

    private void OnMouseEnter()
    {
        if (dialogueRunner != null && dialogueRunner.IsDialogueRunning) return;


    }

    private void OnMouseExit()
    {

    }

    private void OnMouseDown()
    {
        if (dialogueRunner == null || dialogueRunner.IsDialogueRunning) return;

        dialogueRunner.StartDialogue(dialogueNodeName);
    }
}
