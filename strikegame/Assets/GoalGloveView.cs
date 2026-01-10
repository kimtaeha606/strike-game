using UnityEngine;

public sealed class GoalGloveView : MonoBehaviour
{
    [SerializeField] private GoalGlove glove;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite caughtSprite;

    private void OnEnable()
    {
        if (glove != null) glove.OnScored += HandleScored;
    }

    private void OnDisable()
    {
        if (glove != null) glove.OnScored -= HandleScored;
    }

    // ★ 여기 중요
    private void HandleScored(GoalGlove scoredGlove)
    {
        // 필요하면 scoredGlove == glove 체크 가능
        if (sr != null) sr.sprite = caughtSprite;
    }

    public void ResetView()
    {
        if (sr != null) sr.sprite = emptySprite;
    }
}
