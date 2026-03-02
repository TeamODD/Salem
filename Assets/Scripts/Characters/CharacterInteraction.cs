using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(CharacterVisual))]
public class CharacterInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Character Data Asset")]
    [SerializeField] private CharacterData characterData;
    [SerializeField] private DialogueRunner dialogueRunner;

    private static DialogueRunner s_cachedDialogueRunner;

    private CharacterVisual _characterVisual;
    private BoxCollider2D _boxCollider2D;
    private SpriteRenderer _spriteRenderer;
    private CharacterAI _characterAI;
    private GameManager _gameManager;
    private CharacterMark _characterMark;

    private ICharacterFocusController _focusController;
    private IExecutionClickHandler _executionClickHandler;
    private ICharacterDialogueVariableBinder _variableBinder;
    private ICharacterDialogueNodeResolver _nodeResolver;
    private readonly CharacterExecutionState _executionState = new CharacterExecutionState();

    public string CharacterName => characterData != null ? characterData.characterName : string.Empty;

    private void Awake()
    {
        _characterVisual = GetComponent<CharacterVisual>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _characterAI = GetComponent<CharacterAI>();
        _gameManager = GameManager.Instance;
        _characterMark = GetComponentInChildren<CharacterMark>(true);

        _focusController = new CharacterFocusController(_characterVisual);
        _executionClickHandler = new CharacterExecutionClickHandler();
        _variableBinder = new CharacterDialogueVariableBinder();
        _nodeResolver = new CharacterDialogueNodeResolver();
    }

    private void Start()
    {
        dialogueRunner = ResolveDialogueRunner();

        if (characterData != null)
        {
            _characterVisual.Initialize(characterData);
            UpdateColliderToMatchSprite();
        }
        else
        {
            Debug.LogError($"{gameObject.name}의 CharacterData가 설정되지 않았습니다.");
        }

        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueComplete?.AddListener(OnDialogueEnded);
        }
    }

    private DialogueRunner ResolveDialogueRunner()
    {
        if (dialogueRunner != null) return dialogueRunner;
        if (s_cachedDialogueRunner == null)
        {
            s_cachedDialogueRunner = FindFirstObjectByType<DialogueRunner>();
        }

        return s_cachedDialogueRunner;
    }

    private CharacterAI ResolveCharacterAI()
    {
        if (_characterAI == null)
        {
            _characterAI = GetComponent<CharacterAI>();
        }

        return _characterAI;
    }

    private void OnDestroy()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueComplete?.RemoveListener(OnDialogueEnded);
        }
    }

    private void UpdateColliderToMatchSprite()
    {
        if (_spriteRenderer == null || _spriteRenderer.sprite == null) return;

        _boxCollider2D.size = _spriteRenderer.sprite.bounds.size;
        _boxCollider2D.offset = _spriteRenderer.sprite.bounds.center;
    }

    public void SetCharacterData(CharacterData newData)
    {
        if (newData == null) return;
        characterData = newData;

        _characterVisual.Initialize(characterData);
        UpdateColliderToMatchSprite();
        _characterMark?.RefreshCanvasPosition();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UIEventBlocker.IsPointerOverUI) return;

        bool isDialogueRunning = dialogueRunner != null && dialogueRunner.IsDialogueRunning;
        _focusController.OnPointerEnter(isDialogueRunning);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIEventBlocker.IsPointerOverUI) return;

        _focusController.OnPointerExit();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CharacterAI currentAI = ResolveCharacterAI();

        if (_executionClickHandler.TryHandleClick(currentAI, characterData, dialogueRunner, _characterVisual, _executionState))
        {
            if (_executionState.HasShownExecutionDialogue)
            {
                _focusController.LockFocus();
            }
            return;
        }

        if (characterData == null || dialogueRunner == null || dialogueRunner.IsDialogueRunning) return;

        _focusController.LockFocus();

        string baseNodeName = characterData.dialogueNodeName;
        string finalNodeName = baseNodeName;

        if (currentAI != null && currentAI.LastAction != null)
        {
            _variableBinder.BindDefaultTargetName(dialogueRunner, currentAI);

            if (_gameManager == null)
            {
                _gameManager = GameManager.Instance;
            }

            AIContext context = _gameManager != null ? _gameManager.CurrentContext : null;
            DialogueNodeResolution resolution = _nodeResolver.Resolve(currentAI, baseNodeName, context);
            finalNodeName = resolution.NodeName;

            if (resolution.HasTargetNameOverride)
            {
                _variableBinder.SetTargetName(dialogueRunner, resolution.TargetNameOverride);
            }
        }

        if (!dialogueRunner.Dialogue.NodeExists(finalNodeName))
        {
            Debug.LogWarning($"노드 '{finalNodeName}' 가 없습니다.");
            finalNodeName = baseNodeName;
        }

        dialogueRunner.StartDialogue(finalNodeName);
    }

    private void OnDialogueEnded()
    {
        _executionState.HasShownExecutionDialogue = false;
        _focusController.ReleaseFocusLock();
    }
}
