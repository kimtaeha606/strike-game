using UnityEngine;
using System;

public sealed class GameFlow : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private GameState state;

    [Header("UI")]
    [SerializeField] private GameObject tapToStartUI;
    [SerializeField] private GameObject gameOverUI;

    [Header("Refs")]
    [SerializeField] private DragController dragController;

    [Header("Spawn/Director")]
    [SerializeField] private DifficultyDirector difficultyDirector;
    [SerializeField] private GloveSpawner gloveSpawner;

    [Header("Ball")]
    [SerializeField] private Rigidbody2D ballRb;
    [SerializeField] private Transform ballTf;

    [Header("Score")]
    [SerializeField] private int score = 0;

    public int Score => score;
    public event Action<int> OnScoreChanged;

    private const string BestScoreKey = "BEST_SCORE";
    public int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);

    // Runtime gloves (no inspector)
    private GoalGlove currentGlove;
    private GoalGlove nextGlove;

    // 현재 구독 중인 컴포넌트(=currentGlove)
    private GoalGlove subscribedGlove;

    private void Awake()
    {
        // 시작 글러브 자동 탐색
        currentGlove = FindFirstObjectByType<GoalGlove>();
        if (currentGlove == null)
        {
            Debug.LogError("[GameFlow] Scene에 GoalGlove가 없음 (시작 글러브 필요)");
            return;
        }

        SetCurrentGlove(currentGlove);
    }

    private void Start()
    {
        EnterReady();
    }

    private void OnDisable()
    {
        UnsubscribeCurrentGlove();
    }

    private void SetCurrentGlove(GoalGlove newGlove)
    {
        UnsubscribeCurrentGlove();

        currentGlove = newGlove;
        if (currentGlove == null) return;

        // 요청대로: currentGlove에서 컴포넌트 다시 찾기
        subscribedGlove = currentGlove.GetComponent<GoalGlove>();
        if (subscribedGlove == null)
        {
            Debug.LogError("[GameFlow] currentGlove에 GoalGlove 컴포넌트가 없음");
            return;
        }

        subscribedGlove.OnScored += HandleScored;
        subscribedGlove.OnMissed += HandleMissed;

        if (dragController != null) dragController.Glove = currentGlove;

        // 드래그 컨트롤러에 현재 글러브 반영

    }

    private void UnsubscribeCurrentGlove()
    {
        if (subscribedGlove == null) return;

        subscribedGlove.OnScored -= HandleScored;
        subscribedGlove.OnMissed -= HandleMissed;
        subscribedGlove = null;
    }

    public void OnTapToStart()
    {
        if (state != GameState.Ready) return;
        EnterPlaying();
    }

    public void EnterReady()
    {
        state = GameState.Ready;

        if (tapToStartUI != null) tapToStartUI.SetActive(true);
        if (gameOverUI != null) gameOverUI.SetActive(false);

        if (dragController != null) dragController.EnableInput(false);
    }

    private void EnterPlaying()
    {
        state = GameState.Playing;

        if (tapToStartUI != null) tapToStartUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);

        if (dragController != null) dragController.EnableInput(true);

        // 시작 샷 Arm
        ArmCurrentShot();
        EnsureNextGlovePrepared();
    }

    private void EnsureNextGlovePrepared()
    {
        if (nextGlove != null) return;
        if (difficultyDirector == null || gloveSpawner == null || currentGlove == null) return;

        GloveSpec spec = difficultyDirector.GetNextSpec(score);
        nextGlove = gloveSpawner.SpawnNext(currentGlove, spec);
    }

    private void EnterGameOver()
    {
        state = GameState.GameOver;

        if (gameOverUI != null) gameOverUI.SetActive(true);

        if (dragController != null) dragController.EnableInput(false);

        // 샷 해제
        if (subscribedGlove != null)
            subscribedGlove.DisarmShot();
    }

    public void StartNewGame()
    {
        score = 0;
        OnScoreChanged?.Invoke(score);

        // (선택) 공을 현재 글러브 시작점으로 리셋
        if (currentGlove != null)
            ResetBallTo(currentGlove.StartPoint.position);

        // nextGlove 프리로드 제거 (필요시 다시 생성)
        nextGlove = null;

        // Ready에서 시작하도록 하는 설계면 여기서는 Arm 안 해도 됨.
        // Playing 들어갈 때 ArmCurrentShot()가 호출됨.
    }

    private void ResetBallTo(Vector3 worldPos)
    {
        if (ballRb == null) return;

        Transform t = (ballTf != null) ? ballTf : ballRb.transform;

        t.position = worldPos;
        ballRb.linearVelocity = Vector2.zero;
        ballRb.angularVelocity = 0f;

        ballRb.Sleep();
        ballRb.WakeUp();
    }

    private Collider2D GetBallCollider()
    {
        if (ballRb == null) return null;
        return ballRb.GetComponent<Collider2D>();
    }

    private void ArmCurrentShot()
    {
        if (subscribedGlove == null) return;

        Collider2D ballCol = GetBallCollider();
        if (ballCol == null)
        {
            Debug.LogError("[GameFlow] Ball Collider missing (ballRb에 Collider2D 필요)");
            return;
        }

        subscribedGlove.ArmShot(ballCol);
    }

    private void HandleScored(GoalGlove glove)
    {
        if (state != GameState.Playing) return;

        score += 1;
        OnScoreChanged?.Invoke(score);

        // BestScore 갱신
        int best = PlayerPrefs.GetInt(BestScoreKey, 0);
        if (score > best)
        {
            PlayerPrefs.SetInt(BestScoreKey, score);
            PlayerPrefs.Save();
        }

        if (difficultyDirector == null || gloveSpawner == null || currentGlove == null)
        {
            Debug.LogError("[GameFlow] Missing refs (difficultyDirector / gloveSpawner / currentGlove)");
            return;
        }

        // ✅ 이전 current 기록(나중에 삭제/정리 용도)
        GoalGlove oldCurrent = currentGlove;

        // ✅ 이번 샷 종료(기존 current에 대해)
        if (subscribedGlove != null)
            subscribedGlove.DisarmShot();

        // next 없으면 생성
        if (nextGlove == null)
        {
            GloveSpec spec = difficultyDirector.GetNextSpec(score);
            nextGlove = gloveSpawner.SpawnNext(oldCurrent, spec);
            if (nextGlove == null) return;
        }

        // current 교체(구독 대상도 교체)
        SetCurrentGlove(nextGlove);

        // 공 리셋
        ResetBallTo(currentGlove.StartPoint.position);

        // ✅ 다음 샷 시작
        ArmCurrentShot();

        // 다음 글러브 미리 생성
        GloveSpec nextSpec = difficultyDirector.GetNextSpec(score);
        nextGlove = gloveSpawner.SpawnNext(currentGlove, nextSpec);

        // ✅ 이전 글러브 삭제(원하면)
        // 스포너를 "매번 Instantiate" 방식으로 바꿨다면 아래를 켜라.
        // (풀링 1개 구조면 Destroy하면 안 됨)
        //
        //if (oldCurrent != null && oldCurrent != currentGlove)
             //Destroy(oldCurrent.gameObject);
    }

    private void HandleMissed(GoalGlove glove)
    {
        if (state != GameState.Playing) return;
        EnterGameOver();
    }
}
