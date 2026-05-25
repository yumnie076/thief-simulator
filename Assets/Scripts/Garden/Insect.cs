using UnityEngine;

/// <summary>
/// Simple wandering AI for ecosystem insects.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Insect : MonoBehaviour
{
    public enum InsectType { Butterfly, Bug, Spider, Snail, Frog, Bird }
    
    public InsectType type;
    public float moveSpeed = 2f;
    public float wanderRadius = 5f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float waitTime;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.5f);
    }

    private void Start()
    {
        startPos = transform.position;
        PickNewTarget();
        
        // Butterflies and Birds fly over things, bugs/frogs/snails crawl below
        if (type == InsectType.Butterfly || type == InsectType.Bird) sr.sortingOrder = 50;
        else sr.sortingOrder = 5;
    }

    private void Update()
    {
        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // Flip sprite based on direction
        if (targetPos.x < transform.position.x) sr.flipX = true;
        else if (targetPos.x > transform.position.x) sr.flipX = false;

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            InteractWithEnvironment();
            PickNewTarget();
            waitTime = Random.Range(0.5f, 2.5f); // Pause before moving again
        }
    }

    private void InteractWithEnvironment()
    {
        if (GardenManager.Instance == null || ScoreManager.Instance == null) return;

        // E.g., 25% chance to do something when stopping
        if (Random.value < 0.25f)
        {
            if (type == InsectType.Bug) // "Bug" serves as Bee
            {
                // Bees spawn flowers
                GardenManager.Instance.SpawnObject(GardenObject.ObjectType.Flower, transform.position);
                if (ScoreManager.Instance != null) ScoreManager.Instance.AddBiodiversity(2f);
            }
            else if (type == InsectType.Bird)
            {
                // Birds occasionally drop seeds that become bushes
                GardenManager.Instance.SpawnObject(GardenObject.ObjectType.Bush, transform.position);
                if (ScoreManager.Instance != null) ScoreManager.Instance.AddBiodiversity(3f);
            }
            else if (type == InsectType.Frog || type == InsectType.Snail)
            {
                // Frogs/Snails slowly increase biodiversity over time when wandering
                if (ScoreManager.Instance != null) ScoreManager.Instance.AddBiodiversity(1f);
            }
        }
    }

    private void PickNewTarget()
    {
        float rx = Random.Range(-wanderRadius, wanderRadius);
        float ry = Random.Range(-wanderRadius, wanderRadius);
        
        // Keep within garden bounds (roughly)
        float clampedX = Mathf.Clamp(startPos.x + rx, 2f, GardenManager.Instance.GardenWidth - 2f);
        float clampedY = Mathf.Clamp(startPos.y + ry, 2f, GardenManager.Instance.GardenHeight - 2f);
        
        targetPos = new Vector3(clampedX, clampedY, 0);
    }

    public static Sprite GetSpriteForType(InsectType type)
    {
        switch (type)
        {
            case InsectType.Butterfly: return Resources.Load<Sprite>("EgelGame/insect_butterfly");
            case InsectType.Bug:       return Resources.Load<Sprite>("EgelGame/insect_bug");
            case InsectType.Spider:    return Resources.Load<Sprite>("EgelGame/insect_spider");
            case InsectType.Snail:     return Resources.Load<Sprite>("EgelGame/insect_snail");
            case InsectType.Frog:      return Resources.Load<Sprite>("EgelGame/insect_frog");
            case InsectType.Bird:      return Resources.Load<Sprite>("EgelGame/insect_bird");
            default: return null;
        }
    }
}
