using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParallaxBackgroundController : MonoBehaviour
{
    [System.Serializable]
    public struct ParallaxLayer
    {
        public Transform layerTransform;
        public float speed;
    }

    public ParallaxLayer[] backgroundLayers;
    public float smoothTime = 0.1f;

    private Vector3[] _startPositions;

    private void Start()
    {
        _startPositions = new Vector3[backgroundLayers.Length];
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            _startPositions[i] = backgroundLayers[i].layerTransform.localPosition;
        }
    }

    private void Update()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();

        Vector2 normalizedMousePos = new Vector2(
            (mousePosition.x / Screen.width) - 0.5f,
            (mousePosition.y / Screen.height) - 0.5f
        );

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            Vector3 targetPosition = _startPositions[i] + new Vector3(
                -normalizedMousePos.x * backgroundLayers[i].speed,
                -normalizedMousePos.y * backgroundLayers[i].speed,
                _startPositions[i].z
            );

            backgroundLayers[i].layerTransform.localPosition = Vector3.Lerp(
                backgroundLayers[i].layerTransform.localPosition,
                targetPosition,
                Time.deltaTime / smoothTime
            );
        }
    }
}
