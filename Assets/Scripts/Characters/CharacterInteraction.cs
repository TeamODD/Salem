using System.Collections.Generic;
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

    // 런타임에 캐릭터 데이터를 교체하기 위한 메서드
    public void SetCharacterData(CharacterData newData)
    {
        if (newData == null) return;

        characterData = newData;

        if (_characterVisual == null) _characterVisual = GetComponent<CharacterVisual>();
        if (_boxCollider2D == null) _boxCollider2D = GetComponent<BoxCollider2D>();
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

        // 비주얼 및 콜라이더 갱신
        _characterVisual.Initialize(characterData);
        UpdateColliderToMatchSprite();
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
        // 0. 처형 모드 확인
        if (ExecutionManager.Instance != null && ExecutionManager.Instance.IsAiming)
        {
            CharacterAI ai = GetComponent<CharacterAI>();
            if (ai != null)
            {
                ExecutionManager.Instance.ExecuteTarget(ai);
                return; // 대화 시작하지 않고 종료
            }
        }

        if (characterData == null) return;

        if (_dialogueRunner != null && !_dialogueRunner.IsDialogueRunning)
        {
            _isFocusLocked = true;
            _characterVisual.SetFocus(true);

            string finalNodeName = characterData.dialogueNodeName;
            CharacterAI ai = GetComponent<CharacterAI>();

            if (ai != null && ai.LastAction != null)
            {
                // 1. 변수 주입
                CharacterAI effectiveTarget = ai.CurrentLieTarget;

                // 거짓말 대상이 없다면 실제 행동 대상을 사용
                if (effectiveTarget == null && ai.LastAction.Target != null)
                {
                    effectiveTarget = ai.LastAction.Target.GetComponent<CharacterAI>();
                }

                if (effectiveTarget != null)
                {
                    _dialogueRunner.VariableStorage.SetValue("$targetName", effectiveTarget.DisplayName);
                }
                else
                {
                    _dialogueRunner.VariableStorage.SetValue("$targetName", "누군가");
                }

                // 2. 노드 이름 결정
                finalNodeName = DetermineNodeName(ai, characterData.dialogueNodeName);
            }

            // 폴백: 결정된 노드가 존재하지 않으면 경고 출력
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

        // 사칭하는 역할이 있다면 그것을 사용, 아니면 본인 역할 사용
        Role.Roles activeRole = ai.LastAction.PretendRole.HasValue ? ai.LastAction.PretendRole.Value : ai.MyRole;

        switch (activeRole)
        {
            case Role.Roles.신자:
                // 신자가 집에 머물렀거나, 조사를 못 한 경우(타겟 없음) 기본 대사 출력
                if (actionId == "believer_stay_home" || (actionId == "believer_investigate" && ai.LastAction.Target == null))
                {
                    return baseNode; // "별일 없었네.." (기본 대사)
                }
                else if (actionId == "believer_body_found" || actionId == "witch_attack") // 마녀 습격을 시체 발견으로 위장?
                {
                    suffix = "_Believer_BodyFound";
                }
                else if (actionId == "believer_absent")
                {
                    suffix = "_Believer_Absent";
                }
                else
                {
                    // 좀도둑이 신자 흉내를 낼 때, 성공이면 조사 성공으로 간주
                    if (success) suffix = "_Believer_Success";
                    else suffix = "_Believer_Refused";
                }
                break;

            case Role.Roles.불면증:
                if (actionId == "insomniac_walk" || (ai.LastAction.PretendRole.HasValue && success))
                    suffix = "_Insomniac_Out";
                else
                    suffix = "_Insomniac_Home";
                break;

            case Role.Roles.겁쟁이:
                if (actionId == "coward_plea")
                {
                    suffix = "_Coward_Plea";
                }
                break;

            case Role.Roles.벙어리:
                suffix = "_Mute_Silent";
                break;

            case Role.Roles.시민:
                bool received = false;
                if (ai is CitizenAI citizen) received = citizen.HasReceivedPrayer;

                // 마녀가 시민인 척 할 때
                if (ai is WitchAI)
                {
                    // 마녀는 기도를 안 받았다고 가정 (혹은 랜덤)
                    received = false;
                }

                if (received) suffix = "_Received_Prayer";
                else return baseNode; // 기본 대사 출력
                break;
        }

        // 집에 있었으면서(접미사가 없거나 집에 있었음, 또는 이미 기도를 받은 경우), 기도를 받은 경우 체크 (우선순위 높음)
        bool wasHome = string.IsNullOrEmpty(suffix) || suffix == "_Insomniac_Home" || suffix == "_Received_Prayer";

        if (wasHome && AIManager.Instance != null && AIManager.Instance.CurrentContext != null)
        {
            AIContext context = AIManager.Instance.CurrentContext;
            if (context.PrayerReceived.Contains(ai))
            {
                suffix = "_Received_Prayer";

                // 누가 찾아왔는지 확인
                GameObject myObj = ai.gameObject;
                foreach (KeyValuePair<CharacterAI, AIAction> kvp in context.Actions)
                {
                    CharacterAI actor = kvp.Key;
                    AIAction action = kvp.Value;

                    if (action.Target == null) continue;
                    
                    // 타겟의 GameObject 비교
                    if (action.Target.gameObject == myObj)
                    {
                        // 신자 행동이거나, 신자를 사칭했거나, 행동 ID에 신자 조사가 포함된 경우
                        bool isBelieverRole = actor.MyRole == Role.Roles.신자;
                        bool isPretendingBeliever = action.PretendRole == Role.Roles.신자;
                        bool isBelieverActionId = action.ActionId.Contains("believer");

                        if (action.Success && (isBelieverRole || isPretendingBeliever || isBelieverActionId))
                        {
                            if (_dialogueRunner != null)
                            {
                                _dialogueRunner.VariableStorage.SetValue("$targetName", actor.DisplayName);
                            }
                            break;
                        }
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
