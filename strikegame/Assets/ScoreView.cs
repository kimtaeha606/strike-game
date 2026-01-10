//https://chatgpt.com/c/69620046-5314-8321-bad1-66c30253309f
using TMPro;
using UnityEngine;

public sealed class ScoreView : MonoBehaviour
{
    [SerializeField] private GameFlow gameFlow;
    [SerializeField] private TMP_Text scoreText;

    private void Awake()
    {
        if (gameFlow == null) gameFlow = FindFirstObjectByType<GameFlow>();
    }

    private void OnEnable()
    {
        if (gameFlow != null) gameFlow.OnScoreChanged += HandleScoreChanged;

        // 초기 표시 (이벤트를 기다리면 시작 시 0이 안 찍힐 수 있음)
        if (gameFlow != null) HandleScoreChanged(gameFlow.Score);
    }

    private void OnDisable()
    {
        if (gameFlow != null) gameFlow.OnScoreChanged -= HandleScoreChanged;
    }

    private void HandleScoreChanged(int newScore)
    {
        if (scoreText != null) scoreText.text = newScore.ToString();
    }
}
