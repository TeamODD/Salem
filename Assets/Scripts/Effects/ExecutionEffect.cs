using UnityEngine;
using DG.Tweening;

public class ExecutionEffect : MonoBehaviour
{
    public static ExecutionEffect Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public GameObject BloodEffectPrefab;
    public float EffectDuration = 1.0f;
    public float EffectScale = 1.0f;
    public void ExecuteEffect(Vector2 objectPosition)
    {
        if (BloodEffectPrefab != null)
        {
            Debug.Log("이펙트 실행됨");
            Vector2 spawnPos = (Vector2)Camera.main.transform.position;

            GameObject effectObject = Instantiate(BloodEffectPrefab, spawnPos, Quaternion.identity);
            SpriteRenderer effect = effectObject.GetComponent<SpriteRenderer>();
            effect.sortingOrder = 100;
            effect.transform.localScale = Vector3.one * EffectScale;
            effect.DOFade(0f, EffectDuration).OnComplete(() => Destroy(effectObject));    
        }
    }
}
