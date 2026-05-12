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
            radius = 0.25f,
            color = new Color(0.80f, 0.10f, 0.10f),
            score = 1,
        },
        new FruitConfig
        {
            fruitName = "Strawberry",
            radius = 0.35f,
            color = new Color(0.90f, 0.30f, 0.20f),
            score = 3,
        },
        new FruitConfig
        {
            fruitName = "Grape",
            radius = 0.45f,
            color = new Color(0.50f, 0.20f, 0.70f),
            score = 6,
        },
        new FruitConfig
        {
            fruitName = "Tangerine",
            radius = 0.55f,
            color = new Color(1.00f, 0.60f, 0.10f),
            score = 10,
        },
        new FruitConfig
        {
            fruitName = "Peach",
            radius = 0.65f,
            color = new Color(0.90f, 0.40f, 0.10f),
            score = 15,
        },
        new FruitConfig
        {
            fruitName = "Apple",
            radius = 0.75f,
            color = new Color(0.90f, 0.10f, 0.10f),
            score = 21,
        },
        new FruitConfig
        {
            fruitName = "Pair",
            radius = 0.90f,
            color = new Color(0.90f, 0.90f, 0.40f),
            score = 28,
        },
        new FruitConfig
        {
            fruitName = "Peach",
            radius = 1.05f,
            color = new Color(1.00f, 0.70f, 0.70f),
            score = 36,
        },
        new FruitConfig
        {
            fruitName = "Pineapple",
            radius = 1.20f,
            color = new Color(0.90f, 0.80f, 0.10f),
            score = 45,
        },
        new FruitConfig
        {
            fruitName = "Melon",
            radius = 1.40f,
            color = new Color(0.50f, 0.90f, 0.40f),
            score = 55,
        },
        new FruitConfig
        {
            fruitName = "Watermelon",
            radius = 1.60f,
            color = new Color(0.20f, 0.70f, 0.20f),
            score = 66,
        },
    };

    public static Sprite CircleSprite { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action OnGameOver;

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
        CircleSprite = CreateCircleSprite();
    }

    private static Sprite CreateCircleSprite()
    {
        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float c = size * 0.5f;
        float r = c - 1f;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x - c,
                dy = y - c;
            pixels[y * size + x] = (dx * dx + dy * dy <= r * r) ? Color.white : Color.clear;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
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

    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
