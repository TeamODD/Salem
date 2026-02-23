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

    private Canvas _parentCanvas;

    void Start()
    {
        if (RoleGuessManager.Instance != null)
        {
            RoleGuessManager.Instance.RegisterMark(this);
        }

        // Canvas를 캐릭터 머리 위에 자동 배치
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

        // 스프라이트의 월드 높이 계산 (캐릭터 스케일 반영)
        float spriteHeight = spriteRenderer.sprite.bounds.size.y;

        // Canvas의 로컬 position을 스프라이트 상단 + 오프셋으로 설정
        // Canvas는 Character의 자식이므로 localPosition 사용
        // 스프라이트의 pivot 기준 상단 = bounds.max.y
        float topY = spriteRenderer.sprite.bounds.max.y;
        _parentCanvas.transform.localPosition = new Vector3(0f, topY + _headOffset, 0f);

        // World Space에 맞는 스케일 적용
        _parentCanvas.transform.localScale = new Vector3(_canvasWorldScale, _canvasWorldScale, _canvasWorldScale);
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
