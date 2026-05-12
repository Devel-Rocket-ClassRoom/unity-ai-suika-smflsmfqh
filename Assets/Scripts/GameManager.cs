using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager { get; private set; }

    [Serializable]
    public class FruitConfig
    {
        public string fruitName;
        public float radius;
        public Color color;
        public int score;
    }

    public static readonly FruitConfig[] Configs = new FruitConfig[]
    {
        new FruitConfig
        {
            fruitName = "Cherry",
            radius = 0.375f,
            color = new Color(0.80f, 0.10f, 0.10f),
            score = 1,
        },
        new FruitConfig
        {
            fruitName = "Strawberry",
            radius = 0.525f,
            color = new Color(0.90f, 0.30f, 0.20f),
            score = 3,
        },
        new FruitConfig
        {
            fruitName = "Grape",
            radius = 0.675f,
            color = new Color(0.50f, 0.20f, 0.70f),
            score = 6,
        },
        new FruitConfig
        {
            fruitName = "Tangerine",
            radius = 0.825f,
            color = new Color(1.00f, 0.60f, 0.10f),
            score = 10,
        },
        new FruitConfig
        {
            fruitName = "Peach",
            radius = 0.975f,
            color = new Color(0.90f, 0.40f, 0.10f),
            score = 15,
        },
        new FruitConfig
        {
            fruitName = "Apple",
            radius = 1.125f,
            color = new Color(0.90f, 0.10f, 0.10f),
            score = 21,
        },
        new FruitConfig
        {
            fruitName = "Pair",
            radius = 1.35f,
            color = new Color(0.90f, 0.90f, 0.40f),
            score = 28,
        },
        new FruitConfig
        {
            fruitName = "Peach",
            radius = 1.575f,
            color = new Color(1.00f, 0.70f, 0.70f),
            score = 36,
        },
        new FruitConfig
        {
            fruitName = "Pineapple",
            radius = 1.80f,
            color = new Color(0.90f, 0.80f, 0.10f),
            score = 45,
        },
        new FruitConfig
        {
            fruitName = "Melon",
            radius = 2.10f,
            color = new Color(0.50f, 0.90f, 0.40f),
            score = 55,
        },
        new FruitConfig
        {
            fruitName = "Watermelon",
            radius = 2.40f,
            color = new Color(0.20f, 0.70f, 0.20f),
            score = 66,
        },
    };

    [SerializeField]
    private Sprite[] fruitSprites;

    public static Sprite GetFruitSprite(int level) => gameManager?.fruitSprites[level];

    public event Action<int> OnScoreChanged;
    public event Action OnGameOver;
    public event Action OnGameReset;

    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (gameManager != null && gameManager != this)
        {
            Destroy(gameObject);
            return;
        }
        gameManager = this;
    }

    public void AddScore(int amount)
    {
        if (IsGameOver)
            return;
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    public void TriggerGameOver()
    {
        if (IsGameOver)
            return;
        IsGameOver = true;
        OnGameOver?.Invoke();
    }

    public void ResetGame()
    {
        IsGameOver = false;
        Score = 0;
        OnScoreChanged?.Invoke(0);
        if (FruitSpawner.fruitSpawner != null)
            FruitSpawner.fruitSpawner.ResetAll();
        OnGameReset?.Invoke();
    }
}
