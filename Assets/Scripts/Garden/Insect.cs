using UnityEngine;

/// <summary>
/// Simple wandering AI for ecosystem insects.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Insect : MonoBehaviour
{
    public enum InsectType { Butterfly, Bug, Spider }
    
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
        
        // Butterflies fly over things, bugs crawl below
        if (type == InsectType.Butterfly) sr.sortingOrder = 50;
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
            PickNewTarget();
            waitTime = Random.Range(0.5f, 2.5f); // Pause before moving again
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
            default: return null;
        }
    }
}
