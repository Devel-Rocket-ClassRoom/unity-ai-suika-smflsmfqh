using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PolygonCollider2D), typeof(SpriteRenderer))]
public class Fruit : MonoBehaviour
{
    public int Level { get; private set; }
    public bool IsActive { get; private set; }
    public static readonly List<Fruit> ActiveFruits = new List<Fruit>();

    [SerializeField]
    private float bounceSpeedRef = 6f;

    [SerializeField]
    private float minBounceForce = 0.2f;

    [SerializeField]
    private float maxBounceForce = 2.0f;

    private bool isMerging;
    private Rigidbody2D rb;
    private PolygonCollider2D col;
    private SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<PolygonCollider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (IsActive)
            ActiveFruits.Add(this);
    }

    private void OnDisable() => ActiveFruits.Remove(this);

    public void Init(int level)
    {
        isMerging = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        Level = level;
        var cfg = GameManager.Configs[level];
        transform.localScale = Vector3.one * cfg.radius * 2f;

        var sprite = GameManager.GetFruitSprite(level);
        sr.sprite = sprite;
        sr.color = Color.white;

        if (sprite != null)
        {
            int shapeCount = sprite.GetPhysicsShapeCount();
            col.pathCount = shapeCount;
            var points = new List<Vector2>();
            for (int i = 0; i < shapeCount; i++)
            {
                sprite.GetPhysicsShape(i, points);
                col.SetPath(i, points.ToArray());
            }
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        col.enabled = false;
    }

    public void Drop()
    {
        IsActive = true;
        ActiveFruits.Add(this);
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 2f;
        col.enabled = true;
    }

    public void Deactivate()
    {
        ActiveFruits.Remove(this);
        IsActive = false;
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D col2)
    {
        if (!IsActive || isMerging)
            return;

        var other = col2.gameObject.GetComponent<Fruit>();

        if (
            other != null
            && other.IsActive
            && !other.isMerging
            && other.Level == Level
            && Level < GameManager.Configs.Length - 1
        )
        {
            isMerging = true;
            other.isMerging = true;

            var mid = ((Vector2)transform.position + (Vector2)other.transform.position) * 0.5f;
            if (GameManager.gameManager)
                GameManager.gameManager.AddScore(GameManager.Configs[Level + 1].score);
            if (FruitSpawner.fruitSpawner)
                FruitSpawner.fruitSpawner.SpawnAt(Level + 1, mid);

            if (FruitSpawner.fruitSpawner)
                FruitSpawner.fruitSpawner.Release(other);
            if (FruitSpawner.fruitSpawner)
                FruitSpawner.fruitSpawner.Release(this);
            return;
        }

        if (other != null && col2.contactCount > 0)
        {
            float t = Mathf.Clamp01(col2.relativeVelocity.magnitude / bounceSpeedRef);
            float force = Mathf.Lerp(minBounceForce, maxBounceForce, t);
            rb.AddForce(col2.GetContact(0).normal * force, ForceMode2D.Impulse);
        }
    }
}
