using UnityEngine;

/// <summary>
/// Predator Fox AI that hunts the Hedgehog.
/// </summary>
public class FoxAI : MonoBehaviour
{
    public float wanderSpeed = 0.8f;
    public float chaseSpeed = 1.5f;
    public float detectionRange = 5.5f;

    private Vector3 targetPos;
    private float waitTime;
    private SpriteRenderer sr;
    private Transform hedgehog;
    
    public enum FoxState { Wander, Chase, Investigate }
    public FoxState currentState = FoxState.Wander;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        FindHedgehog();
        PickRandomWanderTarget();
        waitTime = 6f; // Delay initial movement so player has time to orient

        if (GameManager.Instance != null)
        {
            int state = GameManager.Instance.GardenStartState;
            if (state == 1) // Medium (Level 2)
            {
                wanderSpeed *= 1.3f;
                chaseSpeed *= 1.4f;
            }
            else if (state == 0) // Hard (Level 3)
            {
                wanderSpeed *= 1.8f;
                chaseSpeed *= 1.8f;
                detectionRange = 7f; // Also sees you from further away
            }
        }
    }

    private void Update()
    {
        if (hedgehog == null)
        {
            FindHedgehog();
            return;
        }

        // Depth sorting
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10f) + 1; // render slightly above things

        // Check if hedgehog is hidden
        bool hedgehogIsHidden = false;
        var hAI = hedgehog.GetComponent<HedgehogAI>();
        if (hAI != null && hAI.isHidden)
        {
            hedgehogIsHidden = true;
        }

        float distToHedgehog = Vector3.Distance(transform.position, hedgehog.position);

        // State transitions
        if (hedgehogIsHidden)
        {
            if (currentState == FoxState.Chase)
            {
                // Lost sight, go investigate the house position where they hid
                currentState = FoxState.Investigate;
                targetPos = hedgehog.position; 
                waitTime = 2.5f;
            }
        }
        else if (distToHedgehog < detectionRange)
        {
            if (currentState != FoxState.Chase && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAlert();
            }
            currentState = FoxState.Chase;
        }
        else if (currentState == FoxState.Chase)
        {
            // Gave up chase
            currentState = FoxState.Wander;
            PickRandomWanderTarget();
        }

        // State actions
        switch (currentState)
        {
            case FoxState.Wander:
                DoWander();
                break;
            case FoxState.Chase:
                DoChase(distToHedgehog);
                break;
            case FoxState.Investigate:
                DoInvestigate();
                break;
        }
    }

    private void FindHedgehog()
    {
        var h = GameObject.Find("Hedgehog");
        if (h != null) hedgehog = h.transform;
    }

    private void DoWander()
    {
        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
            // Idle: settle rotation
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * 8f);
            sr.color = Color.Lerp(sr.color, Color.white, Time.deltaTime * 5f);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPos, wanderSpeed * Time.deltaTime);
        FlipSprite(targetPos.x);

        // Walking sway
        float sway = Mathf.Sin(Time.time * 8f) * 5f;
        transform.rotation = Quaternion.Euler(0, 0, sway);
        sr.color = Color.Lerp(sr.color, Color.white, Time.deltaTime * 5f);

        // Depth sorting
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10f) + 51;

        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            PickRandomWanderTarget();
            waitTime = Random.Range(1f, 3.5f);
        }
    }

    private bool hasCaught = false;

    private void DoChase(float distToHedgehog)
    {
        if (hasCaught) return;

        targetPos = hedgehog.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
        FlipSprite(targetPos.x);

        // Aggressive chase sway (faster, wider)
        float sway = Mathf.Sin(Time.time * 16f) * 10f;
        transform.rotation = Quaternion.Euler(0, 0, sway);

        // Red tint flash during chase
        float flash = Mathf.Sin(Time.time * 6f) * 0.5f + 0.5f;
        sr.color = Color.Lerp(Color.white, new Color(1f, 0.5f, 0.4f), flash * 0.4f);

        // Depth sorting
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10f) + 51;

        // Check if caught the hedgehog
        if (distToHedgehog < 0.45f)
        {
            var hAI = hedgehog.GetComponent<HedgehogAI>();
            if (hAI != null)
            {
                hasCaught = true;
                hAI.GetCaughtByFox();
            }
        }
    }

    private void DoInvestigate()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, wanderSpeed * Time.deltaTime);
        FlipSprite(targetPos.x);

        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            if (waitTime > 0)
            {
                waitTime -= Time.deltaTime;
                return;
            }
            currentState = FoxState.Wander;
            PickRandomWanderTarget();
        }
    }

    private void PickRandomWanderTarget()
    {
        if (GardenManager.Instance == null) return;
        float rx = Random.Range(2f, GardenManager.Instance.GardenWidth - 2f);
        float ry = Random.Range(2f, GardenManager.Instance.GardenHeight - 2f);
        targetPos = new Vector3(rx, ry, 0);
    }

    private void FlipSprite(float targetX)
    {
        if (targetX < transform.position.x) sr.flipX = true;
        else if (targetX > transform.position.x) sr.flipX = false;
    }
}
