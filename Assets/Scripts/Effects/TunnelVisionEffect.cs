using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using UnityEngine.InputSystem;

public class TunnelVisionEffect : MonoBehaviour
{
    [SerializeField] private Volume _globalVolume;
    private Vignette _vignette;
    private bool _isProcessing = false;
    private const string ID = "LoadIntensity";
    void Start()
    {
        if (_globalVolume.profile.TryGet<Vignette>(out var tmpVignette))
        {
            _vignette = tmpVignette;
        }
    }
    void Update()
    {
        if (_isProcessing && _vignette != null)
        {
            // Mouse.current를 사용하여 좌표 획득
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // 화면 좌표를 뷰포트 좌표(0~1)로 변환
            Vector2 viewportPos = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
            _vignette.center.Override(viewportPos);
        }
    }
    public void StartTunnelVision()
    {
        Debug.Log("Start Tunnel Vision Effect");
        _isProcessing = true;

        DOTween.Kill(ID);

        DOTween.To(() => _vignette.intensity.value,
                x => _vignette.intensity.value = x,
                0.4f,
                0.3f)
            .SetEase(Ease.OutQuad)
            .SetId(ID)
            .OnComplete(() =>
            {
                DOTween.To(() => _vignette.intensity.value,
                           x => _vignette.intensity.value = x,
                           0.5f,
                           0.5f)
                       .SetEase(Ease.InOutSine)
                       .SetLoops(-1, LoopType.Yoyo)
                       .SetId(ID);
            });

    }

    public void ResetVision()
    {
        _isProcessing = false;

        DOTween.Kill(ID);

        DOTween.To(() => _vignette.intensity.value,
                   x => _vignette.intensity.value = x,
                   0f,
                   0.5f)
                   .SetId(ID);
    }
}