using UnityEngine;

public sealed class GloveSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GoalGlove glovePrefab;

    [Header("Placement")]
    [SerializeField] private float minY = -3.5f;
    [SerializeField] private float maxY = 3.5f;

    [Tooltip("x에 약간 랜덤을 주고 싶으면 사용. 0이면 고정")]
    [SerializeField] private float xJitter = 0f;

    [Tooltip("y 랜덤을 덜 흔들고 싶으면 사용. 1=그대로, 0=항상 0")]
    [Range(0f, 1f)]
    [SerializeField] private float yRandomness = 1f;

    private GoalGlove pooledNext;

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

        if (pooledNext == null)
            pooledNext = Instantiate(glovePrefab);

        Vector3 basePos = current.transform.position;

        float jitterX = (xJitter <= 0f) ? 0f : Random.Range(-xJitter, xJitter);
        float x = basePos.x + spec.gapX + jitterX;

        float rawY = Random.Range(minY, maxY);
        float y = Mathf.Lerp(0f, rawY, yRandomness);

        pooledNext.transform.position = new Vector3(x, y, 0f);

        float s = Mathf.Max(0.01f, spec.scale);
        pooledNext.transform.localScale = Vector3.one * s;

       //  4) (선택) 움직임 파라미터 적용: GloveMover가 있으면 세팅
        //var mover = pooledNext.GetComponent<GloveMover>();
        //if (mover != null)
        //{
        //    mover.SetParams(spec.moveAmpX, spec.moveAmpY, spec.moveSpeed);
        //}

        return pooledNext;
    }
}
    

