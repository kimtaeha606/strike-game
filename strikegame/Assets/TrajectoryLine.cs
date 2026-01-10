using UnityEngine;

public sealed class TrajectoryLine : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private LineRenderer line;
    [SerializeField] private Rigidbody2D rb;

    [Header("Dots")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private int dotCount = 20;
    [SerializeField] private float dotTimeStep = 0.1f;
    private GameObject[] dots;

    [Header("Sampling")]
    [SerializeField] private int pointCount = 30;
    [SerializeField] private float timeStep = 0.05f;

    [Header("Preview Limit")]
    [SerializeField] private float maxPreviewTime = 1.2f;        // 이 시간까지만 보여줌
    [SerializeField] private float maxPreviewDistance = 8f;      // 또는 이 거리까지만

    [Header("Fade")]
    [Range(0f, 1f)]
    [SerializeField] private float startAlpha = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float endAlpha = 0f;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        ApplyAlphaGradient();
        if (line != null) { line.enabled = false; line.positionCount = 0; }
        EnsureDots();
        Hide();
    }

    private void OnValidate()
    {
        if (pointCount < 2) pointCount = 2;
        if (timeStep <= 0f) timeStep = 0.02f;
        if (maxPreviewTime <= 0f) maxPreviewTime = 0.2f;
        if (maxPreviewDistance <= 0f) maxPreviewDistance = 1f;
        if (dotCount < 2) dotCount = 2;
        if (dotTimeStep <= 0f) dotTimeStep = 0.05f;

        ApplyAlphaGradient();
    }

    public void Show(Vector2 startPos, Vector2 v0)
    {
        if (rb == null) return;

        if (dotPrefab != null)
        {
            EnsureDots();
            UpdateDots(startPos, v0);
            if (line != null) { line.enabled = false; line.positionCount = 0; }
            return;
        }

        if (line == null) return;
        line.enabled = true;

        Vector2 g = Physics2D.gravity * rb.gravityScale;

        Vector2 prev = startPos;
        int used = 0;

        float maxT = maxPreviewTime;
        float maxD = maxPreviewDistance;
        float maxDSqr = maxD * maxD;

        line.positionCount = pointCount;

        for (int i = 0; i < pointCount; i++)
        {
            float t = i * timeStep;
            if (t > maxT) break;

            Vector2 p = startPos + v0 * t + 0.5f * g * t * t;

            // 시작점 기준 거리 제한
            if ((p - startPos).sqrMagnitude > maxDSqr)
                break;

            line.SetPosition(used++, p);
            prev = p;
        }

        // 최소 2점은 있어야 라인이 보임
        if (used < 2)
        {
            line.positionCount = 0;
            line.enabled = false;
            return;
        }

        line.positionCount = used;
    }

    public void Hide()
    {
        if (line != null)
        {
            line.enabled = false;
            line.positionCount = 0;
        }

        if (dots != null)
        {
            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] != null) dots[i].SetActive(false);
            }
        }
    }

    private void ApplyAlphaGradient()
    {
        if (line == null) return;

        // 색은 유지하고 알파만 그라데이션으로
        Color baseStart = line.startColor;
        Color baseEnd = line.endColor;

        Gradient g = new Gradient();

        g.SetKeys(
            new[]
            {
                new GradientColorKey(baseStart, 0f),
                new GradientColorKey(baseEnd,   1f)
            },
            new[]
            {
                new GradientAlphaKey(startAlpha, 0f),
                new GradientAlphaKey(endAlpha,   1f)
            }
        );

        line.colorGradient = g;
    }

    private void EnsureDots()
    {
        if (dotPrefab == null) return;
        if (dots != null && dots.Length == dotCount) return;

        if (dots != null)
        {
            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] == null) continue;
                if (Application.isPlaying)
                    Destroy(dots[i]);
                else
                    DestroyImmediate(dots[i]);
            }
        }

        dots = new GameObject[dotCount];
        for (int i = 0; i < dotCount; i++)
        {
            dots[i] = Instantiate(dotPrefab, transform);
            dots[i].SetActive(false);
        }
    }

    private void UpdateDots(Vector2 startPos, Vector2 v0)
    {
        if (dots == null || dots.Length == 0) return;

        Vector2 g = Physics2D.gravity * rb.gravityScale;

        for (int i = 0; i < dots.Length; i++)
        {
            float t = i * dotTimeStep;
            Vector2 p = startPos + v0 * t + 0.5f * g * t * t;
            dots[i].transform.position = p;
            dots[i].SetActive(true);
        }
    }
}
