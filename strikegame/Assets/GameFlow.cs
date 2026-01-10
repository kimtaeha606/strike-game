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

    [Header("Refs")]
    [SerializeField] private GoalGlove goalGlove;
    [SerializeField] private DifficultyDirector difficultyDirector;
    [SerializeField] private GloveSpawner gloveSpawner;

    [SerializeField] private GoalGlove currentGlove;
    private GoalGlove nextGlove;
    [Header("Score")]
    [SerializeField] private int score = 0;

    [SerializeField] private Rigidbody2D ballRb;
    [SerializeField] private Transform ballTf;
    public int Score => score;

    // (선택) UI가 구독할 수 있게 점수 변경 이벤트 제공
    public event Action<int> OnScoreChanged;

    private const string BestScoreKey = "BEST_SCORE";

    public int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);



    private void Start()
    {
        EnterReady();
    }

    public void OnTapToStart()
    {
        if (state != GameState.Ready)
            return;
        EnterPlaying();
    }

    public void EnterReady()
    {
        state = GameState.Ready;

        // UI
        if (tapToStartUI != null)
            tapToStartUI.SetActive(true);

        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        // �Է� / ���� ����
        if (dragController != null)
            dragController.EnableInput(false);
    }

    private void EnterPlaying()
    {
        state = GameState.Playing;

        // UI
        if (tapToStartUI != null)
            tapToStartUI.SetActive(false);

        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        // �Է� ���
        if (dragController != null)
            dragController.EnableInput(true);

        
    }

    private void EnterGameOver()
    {
        state = GameState.GameOver;

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        if (dragController != null)
            dragController.EnableInput(false);
    }

        private void OnEnable()
    {
        if (goalGlove != null)
        {
            goalGlove.OnScored += HandleScored;
            goalGlove.OnMissed += HandleMissed;
        }
    }

    private void OnDisable()
    {
        if (goalGlove != null)
        {
            goalGlove.OnScored -= HandleScored;
            goalGlove.OnMissed -= HandleMissed;
        }
    }

    public void StartNewGame()
    {
        score = 0;
        OnScoreChanged?.Invoke(score);
    }

    private void ResetBallTo(Vector3 worldPos)
    {
        if (ballRb == null) return;

        Transform t = (ballTf != null) ? ballTf : ballRb.transform;

        // 위치 스냅
        t.position = worldPos;

        // 물리 초기화
        ballRb.linearVelocity = Vector2.zero;
        ballRb.angularVelocity = 0f;

        // (선택) 물리 안정화
        ballRb.Sleep();
        ballRb.WakeUp();
    }

    private void HandleScored(GoalGlove glove)
    {
        score += 1;
        OnScoreChanged?.Invoke(score);

        // (저장) BestScore 갱신
        int best = PlayerPrefs.GetInt(BestScoreKey, 0);
        if (score > best)
        {
            PlayerPrefs.SetInt(BestScoreKey, score);
            PlayerPrefs.Save();
        }

        if (nextGlove == null)
        {
            GloveSpec spec = difficultyDirector.GetNextSpec(score);
            nextGlove = gloveSpawner.SpawnNext(currentGlove, spec);
            if (nextGlove == null) return;
        }

        currentGlove = nextGlove;

        if (dragController != null)
            dragController.SetCurrentGlove(currentGlove);

        ResetBallTo(currentGlove.StartPoint.position);
        goalGlove.DisarmShot();

        GloveSpec nextSpec = difficultyDirector.GetNextSpec(score);
        nextGlove = gloveSpawner.SpawnNext(currentGlove, nextSpec);
    }

    private void HandleMissed(GoalGlove glove)
    {
        EnterGameOver();
    }

    private void SpawnNextGlove(int score)
    {
        if (difficultyDirector == null || gloveSpawner == null)
        {
            {
                Debug.LogError("[GameFlow] Missing refs (difficultyDirector / gloveSpawner / currentGlove)");
                return;
            }
        }
        GloveSpec spec = difficultyDirector.GetNextSpec(score);

        nextGlove = gloveSpawner.SpawnNext(currentGlove,spec);
    }
}
