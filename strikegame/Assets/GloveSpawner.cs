using UnityEngine;

public sealed class GloveSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GoalGlove glovePrefab;

    [Header("Placement")]
    [Tooltip("Screen padding in world units.")]
    [SerializeField] private float screenPadding = 0.5f;

    [Tooltip("Extra random offset on X.")]
    [SerializeField] private float xJitter = 0f;

    [Range(0f, 1f)]
    [SerializeField] private float yRandomness = 1f;

    [Header("Aim")]
    [SerializeField] private Transform ballTarget;
    public Transform BallTarget
    {
        get => ballTarget;
        set => ballTarget = value;
    }

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    public GoalGlove SpawnNext(GoalGlove current, GloveSpec spec)
    {
        if (glovePrefab == null) return null;
        if (cam == null) cam = Camera.main;
        if (cam == null) return null;

        GoalGlove next = Instantiate(glovePrefab);

        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        float halfW = 0f;
        float halfH = 0f;

        var sr = next.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            halfW = sr.bounds.extents.x;
            halfH = sr.bounds.extents.y;
        }

        float minX = min.x + screenPadding + halfW;
        float maxX = max.x - screenPadding - halfW;
        float minY = min.y + screenPadding + halfH;
        float maxY = max.y - screenPadding - halfH;

        float rawX = Random.Range(minX, maxX);
        float rawY = Mathf.Lerp(0f, Random.Range(minY, maxY), yRandomness);
        float jitterX = (xJitter <= 0f) ? 0f : Random.Range(-xJitter, xJitter);

        float x = Mathf.Clamp(rawX + jitterX, minX, maxX);
        float y = Mathf.Clamp(rawY, minY, maxY);

        next.transform.position = new Vector3(x, y, 0f);

        AimMinusXAtTarget(next.transform, ballTarget);

        return next;
    }

    public void Despawn(GoalGlove glove)
    {
        if (glove == null) return;
        Destroy(glove.gameObject);
    }

    private static void AimMinusXAtTarget(Transform glove, Transform target)
    {
        if (glove == null || target == null) return;

        Vector3 toTarget = target.position - glove.position;
        if (toTarget.sqrMagnitude <= 0f) return;

        // Make local -X point toward target.
        glove.right = -toTarget.normalized;
    }
}
