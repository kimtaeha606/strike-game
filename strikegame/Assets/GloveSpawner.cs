using UnityEngine;

public sealed class GloveSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GoalGlove glovePrefab;

    [Header("Placement")]
    [SerializeField] private float minY = -3.5f;
    [SerializeField] private float maxY = 3.5f;

    [Tooltip("x축에 랜덤 흔들림. 0이면 없음")]
    [SerializeField] private float xJitter = 0f;

    [Tooltip("y 랜덤값을 얼마나 반영할지. 1=그대로, 0=항상 0")]
    [Range(0f, 1f)]
    [SerializeField] private float yRandomness = 1f;

    public GoalGlove SpawnNext(GoalGlove current, GloveSpec spec)
    {
        if (glovePrefab == null)
        {
            Debug.LogError("[GloveSpawner] glovePrefab is null");
            return null;
        }

        if (current == null)
        {
            Debug.LogError("[GloveSpawner] current is null");
            return null;
        }

        // ✅ 매번 새로 생성
        GoalGlove next = Instantiate(glovePrefab);

        Vector3 basePos = current.transform.position;

        float jitterX = (xJitter <= 0f) ? 0f : Random.Range(-xJitter, xJitter);
        float x = basePos.x + spec.gapX + jitterX;

        float rawY = Random.Range(minY, maxY);
        float y = Mathf.Lerp(0f, rawY, yRandomness);

        next.transform.position = new Vector3(x, y, 0f);

        float s = Mathf.Max(0.01f, spec.scale);
        next.transform.localScale = Vector3.one * s;

        return next;
    }

    public void Despawn(GoalGlove glove)
    {
        if (glove == null) return;
        Destroy(glove.gameObject);
    }
}
