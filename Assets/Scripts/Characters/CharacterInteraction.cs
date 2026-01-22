using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(CharacterVisual))]
public class CharacterInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Character Data Asset")]
    [SerializeField] private CharacterData characterData;

    private DialogueRunner dialogueRunner;
    private CharacterVisual characterVisual;
    private BoxCollider2D boxCollider2D;
    private SpriteRenderer spriteRenderer;

    private bool isMouseOver = false;
    private bool isFocusLocked = false;

    private void Start()
    {
        characterVisual = GetComponent<CharacterVisual>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();

        if (characterData != null)
        {
            characterVisual.Initialize(characterData);
            UpdateColliderToMatchSprite();
        }
        else
        {
            Debug.LogError($"{gameObject.name}에 CharacterData가 할당되지 않았습니다");
        }

        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueEnded);
        }
    }

    private void UpdateColliderToMatchSprite()
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            boxCollider2D.size = spriteRenderer.sprite.bounds.size;
            boxCollider2D.offset = spriteRenderer.sprite.bounds.center;
        }
    }

    private void OnDestroy()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueEnded);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        if (!isFocusLocked && !dialogueRunner.IsDialogueRunning)
        {
            characterVisual.SetFocus(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        if (!isFocusLocked)
        {
            characterVisual.SetFocus(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (characterData == null) return;

        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
        {
            isFocusLocked = true;
            characterVisual.SetFocus(true);

            dialogueRunner.StartDialogue(characterData.dialogueNodeName);
        }
    }

    private void OnDialogueEnded()
    {
        isFocusLocked = false;
        if (!isMouseOver)
        {
            characterVisual.SetFocus(false);
        }
    }
}