using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(CharacterVisual))]
public class CharacterInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Character Data Asset")]
    [SerializeField] private CharacterData characterData;

    private DialogueRunner _dialogueRunner;
    private CharacterVisual _characterVisual;
    private BoxCollider2D _boxCollider2D;
    private SpriteRenderer _spriteRenderer;

    private bool _isMouseOver = false;
    private bool _isFocusLocked = false;

    private void Start()
    {
        _characterVisual = GetComponent<CharacterVisual>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _dialogueRunner = FindFirstObjectByType<DialogueRunner>();

        if (characterData != null)
        {
            _characterVisual.Initialize(characterData);
            UpdateColliderToMatchSprite();
        }
        else
        {
            Debug.LogError($"{gameObject.name}의 CharacterData가 설정되지 않았습니다.");
        }

        if (_dialogueRunner == null) return;

        _dialogueRunner.onDialogueComplete?.AddListener(OnDialogueEnded);
    }

    private void UpdateColliderToMatchSprite()
    {
        if (_spriteRenderer == null || _spriteRenderer.sprite == null) return;

        _boxCollider2D.size = _spriteRenderer.sprite.bounds.size;
        _boxCollider2D.offset = _spriteRenderer.sprite.bounds.center;
    }

    private void OnDestroy()
    {
        if (_dialogueRunner == null) return;

        _dialogueRunner.onDialogueComplete?.RemoveListener(OnDialogueEnded);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isMouseOver = true;
        if (!_isFocusLocked && !_dialogueRunner.IsDialogueRunning)
        {
            _characterVisual.SetFocus(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isMouseOver = false;
        if (!_isFocusLocked)
        {
            _characterVisual.SetFocus(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 0. Check Execution Mode
        if (ExecutionManager.Instance != null && ExecutionManager.Instance.IsAiming)
        {
            CharacterAI ai = GetComponent<CharacterAI>();
            if (ai != null)
            {
                ExecutionManager.Instance.ExecuteTarget(ai);
                return; // Stop here, do not start dialogue
            }
        }

        if (characterData == null) return;

        if (_dialogueRunner != null && !_dialogueRunner.IsDialogueRunning)
        {
            _isFocusLocked = true;
            _characterVisual.SetFocus(true);

            string finalNodeName = characterData.dialogueNodeName;
            var ai = GetComponent<CharacterAI>();

            if (ai != null && ai.LastAction != null)
            {
                // 1. Inject Variables
                if (ai.LastAction.Target != null)
                {
                    string targetName = ai.LastAction.Target.name;
                    // Try to get a nicer display name if possible (accessing component if needed)
                    _dialogueRunner.VariableStorage.SetValue("$targetName", targetName);
                }
                else
                {
                    _dialogueRunner.VariableStorage.SetValue("$targetName", "누군가");
                }

                // 2. Determine Node Name
                finalNodeName = DetermineNodeName(ai, characterData.dialogueNodeName);
            }

            // Fallback: If the determined node doesn't exist, Yarn Spinner might error or show nothing.
            // // Ideally check if node exists: dialogueRunner.NodeExists(finalNodeName)
            if (!_dialogueRunner.Dialogue.NodeExists(finalNodeName))
            {
                Debug.LogWarning($"노드 '{finalNodeName}' 가 없습니다.");
                finalNodeName = characterData.dialogueNodeName;
            }

            _dialogueRunner.StartDialogue(finalNodeName);
        }
    }

        private string DetermineNodeName(CharacterAI ai, string baseNode)
        {
            if (ai == null || ai.LastAction == null) return baseNode;
    
            string suffix = "";
            string actionId = ai.LastAction.ActionId;
            bool success = ai.LastAction.Success;
    
            // Use PretendRole if it exists, otherwise use MyRole
            Role.Roles activeRole = ai.LastAction.PretendRole.HasValue ? ai.LastAction.PretendRole.Value : ai.MyRole;
    
            switch (activeRole)
            {
                case Role.Roles.신자:
                    // Even if pretending, we try to match the most logical actionId
                    if (actionId == "believer_body_found" || actionId == "witch_attack") // Witch attack can look like found body?
                    {
                        suffix = "_Believer_BodyFound";
                    }
                    else if (actionId == "believer_absent")
                    {
                        suffix = "_Believer_Absent";
                    }
                    else
                    {
                        // For Thief pretending to be Believer, success usually means Believer_Success
                        if (success) suffix = "_Believer_Success";
                        else suffix = "_Believer_Refused";
                    }
                    break;
    
                case Role.Roles.불면증: // Insomniac
                    if (actionId == "insomniac_walk" || (ai.LastAction.PretendRole.HasValue && success)) 
                        suffix = "_Insomniac_Out";
                    else 
                        suffix = "_Insomniac_Home";
                    break;
                
                            case Role.Roles.겁쟁이: // Coward
                                if (actionId == "coward_plea")
                                {
                                    suffix = "_Coward_Plea";
                                }
                                break;
                
                            case Role.Roles.벙어리:
                                suffix = "_Mute_Silent";
                                break;
                        }
                
                        // Check if received prayer (Priority over staying home)            // Condition: Was at home (Empty suffix or Insomniac_Home) AND Received Prayer
            bool wasHome = string.IsNullOrEmpty(suffix) || suffix == "_Insomniac_Home";
            
            if (wasHome && AIManager.Instance != null && AIManager.Instance.CurrentContext != null)
            {
                var context = AIManager.Instance.CurrentContext;
                if (context.PrayerReceived.Contains(ai))
                {
                    suffix = "_Received_Prayer";
                    
                    // Find who visited
                    Character myCharacter = ai.GetComponent<Character>();
                    foreach (var kvp in context.Actions)
                    {
                        var actor = kvp.Key;
                        var action = kvp.Value;
                        
                        // Check if this action targeted me, succeeded, and was a Believer act
                        bool isBelieverAct = actor.MyRole == Role.Roles.신자 || action.PretendRole == Role.Roles.신자;
                        
                        if (action.Target == myCharacter && action.Success && isBelieverAct)
                        {
                            if (_dialogueRunner != null)
                            {
                                _dialogueRunner.VariableStorage.SetValue("$targetName", actor.name);
                            }
                            break;
                        }
                    }
                }
            }
    
            return baseNode + suffix;
        }
    private void OnDialogueEnded()
    {
        _isFocusLocked = false;
        if (!_isMouseOver)
        {
            _characterVisual.SetFocus(false);
        }
    }
}