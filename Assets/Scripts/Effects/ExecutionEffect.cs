using UnityEngine;
using DG.Tweening;

public class ExecutionEffect : MonoBehaviour
{
    public GameObject BloodEffectPrefab;
    public float EffectDuration = 1.0f;
    public void ExecuteEffect(Vector2 objectPosition)
    {
        if (BloodEffectPrefab != null)
        {
            Debug.Log("이펙트 실행됨");

            GameObject effectObject = Instantiate(BloodEffectPrefab, objectPosition, Quaternion.identity);
            SpriteRenderer effect = effectObject.GetComponent<SpriteRenderer>();
            effect.DOFade(0f, EffectDuration).OnComplete(() => Destroy(effectObject));    
        }
    }
}
