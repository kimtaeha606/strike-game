using UnityEngine;


public sealed class GameFlow : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private GameState state;

    [Header("UI")]
    [SerializeField] private GameObject tapToStartUI;
    [SerializeField] private GameObject gameOverUI;

    [Header("Refs")]
    [SerializeField] private DragController dragController;

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

        // 입력 / 스폰 차단
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

        // 입력 허용
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
}
