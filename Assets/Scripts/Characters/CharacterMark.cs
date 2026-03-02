using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMark : MonoBehaviour
{
    [SerializeField] private Image _markImage;
    [SerializeField] private Sprite _defaultMark;

    [Header("Canvas Position Settings")]
    [Tooltip("캐릭터 머리 위에 Canvas를 배치할 때 추가 오프셋 (월드 단위)")]
    [SerializeField] private float _headOffset = 0.3f;

    [Tooltip("World Space Canvas의 스케일 (월드 단위에 맞게 축소)")]
    [SerializeField] private float _canvasWorldScale = 0.01f;
    [SerializeField] private float _canvasForwardOffset = -0.1f;

    private Canvas _parentCanvas;

    private void Awake()
    {
        if (_markImage == null)
        {
            _markImage = GetComponent<Image>();
        }
    }

    void Start()
    {
        if (RoleGuessManager.Instance != null)
        {
            RoleGuessManager.Instance.RegisterMark(this);
        }

        StartCoroutine(SetUpCanvasPositionDelayed());

        // Canvas를 캐릭터 머리 위에 자동 배치
        // SetupCanvasPosition();
    }

    private void OnEnable()
    {
        StartCoroutine(SetUpCanvasPositionDelayed());
    }

    private IEnumerator SetUpCanvasPositionDelayed()
    {
        yield return null; // 한 프레임 대기
        SetupCanvasPosition();
    }

    /// <summary>
    /// 부모 캐릭터의 SpriteRenderer 높이를 기반으로 Canvas를 머리 위에 위치시킵니다.
    /// </summary>
    private void SetupCanvasPosition()
    {
        _parentCanvas = GetComponentInParent<Canvas>();
        if (_parentCanvas == null) return;

        // Canvas가 World Space 모드인지 확인
        if (_parentCanvas.renderMode != RenderMode.WorldSpace) return;

        // 캐릭터(Canvas의 부모)의 SpriteRenderer에서 높이를 가져옴
        Transform characterTransform = _parentCanvas.transform.parent;
        if (characterTransform == null) return;

        SpriteRenderer spriteRenderer = characterTransform.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        // 실제 렌더링 월드 bounds 상단을 기준으로 위치 계산 (스케일/피벗 반영)
        float topY = spriteRenderer.bounds.max.y;
        Vector3 worldTop = new Vector3(
            spriteRenderer.bounds.center.x,
            topY + _headOffset,
            spriteRenderer.bounds.center.z + _canvasForwardOffset);

        _parentCanvas.transform.position = worldTop;

        // World Space에 맞는 스케일 적용
        _parentCanvas.transform.localScale = new Vector3(_canvasWorldScale, _canvasWorldScale, _canvasWorldScale);
    }

    public void RefreshCanvasPosition()
    {
        SetupCanvasPosition();
    }

    public void OnMarkClicked()
    {
        RoleGuessManager.Instance.OpenSelector(this);
    }

    public void SetGuessedRole(Sprite newIcon)
    {
        _markImage.sprite = newIcon != null ? newIcon : _defaultMark;
    }
}
