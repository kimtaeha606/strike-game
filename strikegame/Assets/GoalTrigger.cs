using UnityEngine;

public sealed class GoalTrigger : MonoBehaviour
{
    [SerializeField] private GoalGlove glove;

    private void Awake()
    {
        if (glove == null)
            glove = GetComponentInParent<GoalGlove>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ball")) return;
        if (glove == null) return;

        glove.NotifyGoal(other);
    }
}
