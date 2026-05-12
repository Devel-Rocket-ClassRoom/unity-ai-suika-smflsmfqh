using UnityEngine;

public class GameOverDetector : MonoBehaviour
{
    public Transform dangerLine;
    private float dangerY;
    public float gracePeriod = 3f;

    private float dangerTimer;
    private float startDelay = 2f;

    private void Start()
    {
        if (dangerLine)
            dangerY = dangerLine.position.y;
        else
            Debug.LogError("Danger line reference is missing on GameOverDetector.");
    }

    private void Update()
    {
        if (GameManager.gameManager == null || GameManager.gameManager.IsGameOver)
            return;

        startDelay -= Time.deltaTime;
        if (startDelay > 0)
            return;

        bool inDanger = false;
        foreach (var fruit in Fruit.ActiveFruits)
        {
            if (fruit.IsActive && fruit.transform.position.y >= dangerY)
            {
                inDanger = true;
                break;
            }
        }

        dangerTimer = inDanger ? dangerTimer + Time.deltaTime : 0f;
        if (dangerTimer >= gracePeriod)
            GameManager.gameManager.TriggerGameOver();
    }
}
