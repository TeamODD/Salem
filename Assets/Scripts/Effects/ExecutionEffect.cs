using UnityEngine;

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
        if (BloodEffectPrefab == null)
        {
            Debug.LogWarning("BloodEffectPrefab이 비어있습니다.");
            return;
        }

        Debug.Log("이펙트 실행됨");

        Vector3 spawnPos = new Vector3(0f,0f,0f);
        GameObject effectObject = Instantiate(BloodEffectPrefab, spawnPos, Quaternion.identity);

        SpriteRenderer effect = effectObject.GetComponent<SpriteRenderer>();
        if (effect != null)
            effect.sortingOrder = 100;

        effectObject.transform.localScale = Vector3.one * EffectScale;

        // 프리팹에 설정된 애니메이션 길이를 우선 사용하고, 없으면 기본 지속 시간을 사용한다.
        float destroyDelay = GetAnimationLength(effectObject);
        Destroy(effectObject, destroyDelay);
    }

    private float GetAnimationLength(GameObject effectObject)
    {
        Animator animator = effectObject.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Update(0f);
            AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);

            if (clipInfos != null && clipInfos.Length > 0 && clipInfos[0].clip != null)
                return clipInfos[0].clip.length;
        }

        Animation legacyAnimation = effectObject.GetComponent<Animation>();
        if (legacyAnimation != null && legacyAnimation.clip != null)
            return legacyAnimation.clip.length;

        return EffectDuration;
    }
}
