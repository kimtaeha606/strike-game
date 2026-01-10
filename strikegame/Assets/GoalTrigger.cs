using UnityEngine;

public sealed class GoalTrigger : MonoBehaviour
{
    [SerializeField] private GoalGlove glove;

    private void Awake()
    {
        Debug.Log("난 살아있어");
        if (glove == null)
            glove = GetComponentInParent<GoalGlove>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("트리거 감지는 되는 듯");
        if (!other.CompareTag("Ball")) return;
        if (glove == null) return;

        glove.NotifyGoal(other);
    }
}
