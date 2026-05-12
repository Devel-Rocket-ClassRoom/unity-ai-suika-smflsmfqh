using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    public static FruitSpawner fruitSpawner { get; private set; }
    public Fruit fruitPrefab;

    public float spawnY = 5f;
    public float minX = -2.2f;
    public float maxX = 2.2f;
    public float dropCooldown = 1f;

    private readonly Queue<Fruit> pool = new();
    private Fruit currentFruit;
    private bool canDrop;
    private int nextLevel;
    private Camera cam;

    private void Awake()
    {
        fruitSpawner = this;
        cam = Camera.main;
    }

    private void Start() => PrepareNext();

    private void Update()
    {
        if (GameManager.gameManager == null || GameManager.gameManager.IsGameOver)
            return;
        if (currentFruit == null || !canDrop)
            return;

        float mx = Mathf.Clamp(cam.ScreenToWorldPoint(Input.mousePosition).x, minX, maxX);
        currentFruit.transform.position = new Vector3(mx, spawnY, 0);

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            currentFruit.Drop();
            currentFruit = null;
            canDrop = false;
            StartCoroutine(CooldownThenPrepare());
        }
    }

    private IEnumerator CooldownThenPrepare()
    {
        yield return new WaitForSeconds(dropCooldown);
        PrepareNext();
    }

    private Fruit GetFromPool(int level, Vector2 pos)
    {
        Fruit fruit = pool.Count > 0 ? pool.Dequeue() : Instantiate(fruitPrefab);
        fruit.gameObject.SetActive(true);
        fruit.Init(level);
        fruit.transform.position = pos;
        return fruit;
    }

    public void Release(Fruit fruit)
    {
        fruit.Deactivate();
        pool.Enqueue(fruit);
    }

    private void PrepareNext()
    {
        int level = nextLevel;
        nextLevel = Random.Range(0, 5);
        currentFruit = GetFromPool(level, new Vector2(0, spawnY));
        canDrop = true;
        if (UIManager.uiManager)
            UIManager.uiManager.ShowNextFruit(nextLevel);
    }

    public void SpawnAt(int level, Vector2 pos)
    {
        var fruit = GetFromPool(level, pos);
        fruit.Drop();
    }
}
