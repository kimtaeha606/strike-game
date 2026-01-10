using UnityEngine;

public sealed class FailTrigger : MonoBehaviour
{
    [SerializeField] private GoalGlove glove;

    private void Awake()
    {
        Debug.Log("failtrigger");
        if (glove == null)
            glove = GetComponentInParent<GoalGlove>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ball")) return;
        if (glove == null) return;
        Debug.Log("트리거 감지");

        glove.NotifyFail(other);
    }
}
