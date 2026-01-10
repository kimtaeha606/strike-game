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

    [Header("Score")]
    [SerializeField] private int score = 0;
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

        Debug.Log($"Scored! score={score}, best={BestScore}");
    }

    private void HandleMissed(GoalGlove glove)
    {
        Debug.Log("Missed");
        // 필요하면 여기서 GameOver 처리
    }
}
