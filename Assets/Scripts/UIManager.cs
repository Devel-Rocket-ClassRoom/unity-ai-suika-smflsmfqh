using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager uiManager { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI nextFruitLabel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;

    private void Awake() => uiManager = this;

    private void Start()
    {
        if (restartButton)
        {
            restartButton.onClick.AddListener(OnRestartButton);
        }

        if (GameManager.gameManager != null)
        {
            GameManager.gameManager.OnScoreChanged += UpdateScore;
            GameManager.gameManager.OnGameOver += ShowGameOver;
        }

        UpdateScore(0);
        gameOverPanel?.SetActive(false);
    }

    private void UpdateScore(int score)
    {
        if (scoreText)
            scoreText.text = $"Score: {score:N0}";
    }

    public void ShowNextFruit(int level)
    {
        if (nextFruitLabel)
            nextFruitLabel.text = $"Next: {GameManager.Configs[level].fruitName}";
    }

    private void ShowGameOver()
    {
        if (gameOverPanel)
            gameOverPanel.SetActive(true);
        if (finalScoreText && GameManager.gameManager)
            finalScoreText.text = $"Final Score\n{GameManager.gameManager.Score:N0}";
    }

    public void OnRestartButton() => GameManager.gameManager?.RestartGame();
}
