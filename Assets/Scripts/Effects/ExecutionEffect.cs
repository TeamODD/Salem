using UnityEngine;

public class ExecutionEffect : MonoBehaviour
{
    public GameObject BloodEffectPrefab;
    public float EffectDuration = 1.0f;
    private bool _hasExecuted = false;
    public void ExecuteEffect(Vector2 objectPosition)
    {
        if (BloodEffectPrefab != null && !_hasExecuted)
        {
            _hasExecuted = true;
            Debug.Log("이펙트 실행됨");
            GameObject effect = Instantiate(BloodEffectPrefab, objectPosition, Quaternion.identity);
            Destroy(effect, EffectDuration);
        }
    }
}
