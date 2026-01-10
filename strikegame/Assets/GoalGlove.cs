using System;
using UnityEngine;

/// <summary>
/// GoalGlove
/// 책임: 골/실패 판정(샷 단위 1회) + 이벤트 발행만.
/// 트리거 감지는 GoalTrigger/FailTrigger가 수행하고 Notify로 알려준다.
/// </summary>
public sealed class GoalGlove : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform startPoint;
    public Transform StartPoint => startPoint;

    public event Action<GoalGlove> OnScored;
    public event Action<GoalGlove> OnMissed;

    private bool shotActive = false;
    private bool resolved = false;
    private Collider2D currentBall = null;

    // GameFlow: 샷 시작(이번 샷에서 판정할 공 지정)
    public void ArmShot(Collider2D ball)
    {
        shotActive = true;
        resolved = false;
        currentBall = ball;
    }

    // GameFlow: 샷 종료(다음 샷 준비)
    public void DisarmShot()
    {
        shotActive = false;
        resolved = false;
        currentBall = null;
    }

    // GoalTrigger가 호출
    public void NotifyGoal(Collider2D ball)
    {
        if (!CanResolve(ball)) return;

        resolved = true;
        OnScored?.Invoke(this);
        Debug.Log("기모찌");
    }

    // FailTrigger가 호출
    public void NotifyFail(Collider2D ball)
    {
        if (!CanResolve(ball)) return;

        resolved = true;
        OnMissed?.Invoke(this);
        Debug.Log("이씨발");
    }

    // 공통 가드(중복/다른 공/샷 아님 방지)
private bool CanResolve(Collider2D ball)
{
    if (!shotActive)
    {
        Debug.LogWarning("[CanResolve] false: shotActive == false (ArmShot 호출 안 됨/타이밍 문제)");
        return false;
    }
    if (resolved)
    {
        Debug.LogWarning("[CanResolve] false: resolved == true (이미 판정 끝남)");
        return false;
    }
    if (ball == null)
    {
        Debug.LogWarning("[CanResolve] false: ball == null");
        return false;
    }
    if (ball != currentBall)
    {
        Debug.LogWarning($"[CanResolve] false: ball != currentBall | ball={ball.name}({ball.GetInstanceID()}) currentBall={(currentBall? currentBall.name : "null")}({(currentBall? currentBall.GetInstanceID():0)})");
        return false;
    }
    return true;
}
}
