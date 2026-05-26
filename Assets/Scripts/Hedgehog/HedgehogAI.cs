using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Free-roaming Hedgehog AI for the ecosystem overhaul.
/// Wanders around, hunts bugs, and sleeps.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class HedgehogAI : MonoBehaviour
{
    public enum HedgehogState
    {
        Wander,
        Hunt,
        Eat,
        Sleep
    }

    [Header("Runtime State")]
    public HedgehogState currentState = HedgehogState.Wander;
    public float moveSpeed = 3.5f; // Slightly faster for fun playable controls

    [Header("Playable State")]
    public bool isHidden = false;
    private Vector3 housePos;
    private HedgehogNeeds needs;
    private TMP_Text headerText;

    private SpriteRenderer sr;
    private Insect targetInsect;
    private Vector3 targetPos;
    private float waitTime;
    private float drinkCooldown = 0f;
    
    public int bugsEaten = 0;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        
        // Setup rigidbody for trigger collisions
        var rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.gravityScale = 0f;

        // Setup collider
        var circleCol = GetComponent<CircleCollider2D>();
        circleCol.isTrigger = false; // Hedgehog can be solid or trigger, but Collectible is a trigger
        circleCol.radius = 0.4f;

        needs = gameObject.AddComponent<HedgehogNeeds>();
        
        // Find header
        var hud = GameObject.Find("HedgehogHUD");
    }

    private void Start()
    {
        needs = gameObject.GetComponent<HedgehogNeeds>();
        if (needs == null) needs = gameObject.AddComponent<HedgehogNeeds>();

        // Wire to UI
        var ui = FindAnyObjectByType<HedgehogPhaseUI>();
        if (ui != null)
        {
            needs.OnHungerChanged += (h) => ui.SetHunger(h / 100f);
            needs.OnSafetyChanged += (s) => ui.SetSafety(s > 30f);
            ui.SetHunger(needs.Hunger / 100f);
            ui.SetSafety(true);
            
            // Grab headerText via child lookup on phase UI
            headerText = ui.GetComponentInChildren<TMP_Text>();
        }

        PickRandomWanderTarget();
    }

    private void Update()
    {
        if (needs == null) needs = GetComponent<HedgehogNeeds>();

        // 1. Handle hidden state
        if (isHidden)
        {
            if (sr != null) sr.enabled = false;
            needs.SetSafe(); // keeps safety at 100

            if (ScoreManager.Instance != null && ScoreManager.Instance.ShelterScore < 30f)
                ScoreManager.Instance.AddShelter(6f * Time.deltaTime);

            if (headerText != null)
                headerText.text = "Je schuilt veilig! Druk [SPATIE] om weer naar buiten te gaan.";

            if (Input.GetKeyDown(KeyCode.Space))
            {
                isHidden = false;
                if (sr != null) sr.enabled = true;
                transform.position = housePos + new Vector3(0f, -0.6f, 0f);
                PickRandomWanderTarget();
            }
            return;
        }

        // 2. Playable WASD / Arrow Key control
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(moveX, moveY, 0f).normalized;

        if (inputDir.magnitude > 0.01f)
        {
            transform.position += inputDir * moveSpeed * Time.deltaTime;
            
            // Keep within garden bounds
            if (GardenManager.Instance != null)
            {
                transform.position = new Vector3(
                    Mathf.Clamp(transform.position.x, 0.5f, GardenManager.Instance.GardenWidth - 0.5f),
                    Mathf.Clamp(transform.position.y, 0.5f, GardenManager.Instance.GardenHeight - 0.5f),
                    0f
                );
            }

            if (moveX < 0) sr.flipX = true;
            else if (moveX > 0) sr.flipX = false;

            // Walking wobble animation
            float wobble = Mathf.Sin(Time.time * 12f) * 8f;
            transform.rotation = Quaternion.Euler(0, 0, wobble);

            targetPos = transform.position;
            waitTime = 0f;
        }
        else
        {
            // Settle rotation back to normal when standing still
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * 10f);
            waitTime = 0f;
            targetPos = transform.position;
        }

        // 3. Game mechanics
        EatNearbyInsects();
        DrinkFromPonds();

        // Tick drink cooldown
        if (drinkCooldown > 0f) drinkCooldown -= Time.deltaTime;

        // 4. House and predator interaction
        var nearbyHouse = GetNearbyHouse();
        var fox = FindAnyObjectByType<FoxAI>();
        bool foxChasing = (fox != null && fox.currentState == FoxAI.FoxState.Chase);

        if (foxChasing)
        {
            var hPhaseUI = FindAnyObjectByType<HedgehogPhaseUI>();
            if (hPhaseUI != null) hPhaseUI.SetSafety(false);
        }
        else
        {
            var hPhaseUI = FindAnyObjectByType<HedgehogPhaseUI>();
            if (hPhaseUI != null) hPhaseUI.SetSafety(true);
        }

        if (headerText != null)
        {
            if (nearbyHouse != null)
            {
                headerText.text = "Druk [SPATIE] om te schuilen in het egelhuisje!";
            }
            else if (foxChasing)
            {
                headerText.text = "GEVAAR! Er jaagt een vos op je! Verstop je in een egelhuisje!";
            }
            else
            {
                headerText.text = "Bestuur de egel! Zoek voedsel & water [WASD / Pijltjestoetsen]";
            }
        }

        // Hide input
        if (Input.GetKeyDown(KeyCode.Space) && nearbyHouse != null)
        {
            isHidden = true;
            housePos = nearbyHouse.transform.position;
            transform.position = housePos;
        }

        // Depth sorting
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var collectible = other.GetComponent<Collectible>();
        if (collectible != null)
        {
            if (needs != null) needs.Feed(20f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayBite();
            Destroy(other.gameObject);
            Debug.Log("[HedgehogAI] Ate a snack! +20 Hunger");

            // Eat pulse animation
            StartCoroutine(EatPulse());
            return;
        }

        // Friend hedgehog discovery
        var friend = other.GetComponent<FriendHedgehog>();
        if (friend != null && !friend.found)
        {
            friend.found = true;
            SimpleParticle.SpawnHearts(transform.position, 5);
            ScoreManager.Instance?.AddFood(50f);
            AudioManager.Instance?.PlayBite();
            if (headerText != null)
                headerText.text = "Je hebt een egelvriendje gevonden! +50 punten!";
            Destroy(friend.gameObject, 2f);
        }
    }

    private System.Collections.IEnumerator EatPulse()
    {
        Vector3 orig = transform.localScale;
        float duration = 0.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float s = 1f + Mathf.Sin(t * Mathf.PI) * 0.25f; // Pulse up to 1.25x then back
            transform.localScale = orig * s;
            yield return null;
        }
        transform.localScale = orig;
    }

    private void DoWander()
    {
        if (Random.value < 0.05f)
        {
            FindNearestInsect();
            if (targetInsect != null)
            {
                currentState = HedgehogState.Hunt;
                return;
            }
        }

        MoveTowardsTarget();

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            PickRandomWanderTarget();
            waitTime = Random.Range(1f, 3f);
        }
    }

    private void DoHunt()
    {
        if (targetInsect == null)
        {
            currentState = HedgehogState.Wander;
            return;
        }

        targetPos = targetInsect.transform.position;
        MoveTowardsTarget();
    }

    private void EatNearbyInsects()
    {
        if (EcosystemManager.Instance == null) return;
        
        for (int i = 0; i < EcosystemManager.Instance.AllInsects.Count; i++)
        {
            var insect = EcosystemManager.Instance.AllInsects[i];
            if (insect == null) continue;
            
            // Cannot catch flying butterflies or birds
            if (insect.type == Insect.InsectType.Butterfly || insect.type == Insect.InsectType.Bird) 
                continue;

            if (Vector3.Distance(transform.position, insect.transform.position) < 0.6f)
            {
                EcosystemManager.Instance.RemoveInsect(insect);
                bugsEaten++;
                
                if (needs != null) needs.Feed(30f);
                if (ScoreManager.Instance != null) ScoreManager.Instance.AddFood(15f);
                SimpleParticle.SpawnHearts(transform.position, 2);

                targetInsect = null;
                currentState = HedgehogState.Wander;
                break;
            }
        }
    }

    private void DrinkFromPonds()
    {
        if (GardenManager.Instance == null) return;
        foreach (var obj in GardenManager.Instance.PlacedObjects)
        {
            if (obj.Type == GardenObject.ObjectType.Pond)
            {
                if (Vector3.Distance(transform.position, obj.transform.position) < 1.0f)
                {
                    if (needs != null) needs.Drink(10f * Time.deltaTime);
                    if (ScoreManager.Instance != null && ScoreManager.Instance.WaterScore < 30f)
                        ScoreManager.Instance.AddWater(5f * Time.deltaTime);

                    // Visual + audio feedback on cooldown
                    if (drinkCooldown <= 0f)
                    {
                        SimpleParticle.SpawnSplash(transform.position);
                        AudioManager.Instance?.PlaySplash();
                        drinkCooldown = 2f;
                    }
                }
            }
        }
    }

    private GardenObject GetNearbyHouse()
    {
        if (GardenManager.Instance == null) return null;
        foreach (var obj in GardenManager.Instance.PlacedObjects)
        {
            if (obj.Type == GardenObject.ObjectType.HedgehogHouse)
            {
                if (Vector3.Distance(transform.position, obj.transform.position) < 1.0f)
                {
                    return obj;
                }
            }
        }
        return null;
    }

    public void GetCaughtByFox()
    {
        Debug.Log("[HedgehogAI] Fled from fox!");
        PhaseController.Instance?.StartPhase(PhaseController.GamePhase.Result);
    }

    private void FindNearestInsect()
    {
        float bestDist = 8f;
        targetInsect = null;

        if (EcosystemManager.Instance == null) return;

        foreach (var insect in EcosystemManager.Instance.AllInsects)
        {
            if (insect.type == Insect.InsectType.Butterfly || insect.type == Insect.InsectType.Bird) 
                continue;

            float dist = Vector3.Distance(transform.position, insect.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                targetInsect = insect;
            }
        }
    }

    private void PickRandomWanderTarget()
    {
        if (GardenManager.Instance == null) return;
        float rx = Random.Range(2f, GardenManager.Instance.GardenWidth - 2f);
        float ry = Random.Range(2f, GardenManager.Instance.GardenHeight - 2f);
        targetPos = new Vector3(rx, ry, 0);
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        if (targetPos.x < transform.position.x) sr.flipX = true;
        else if (targetPos.x > transform.position.x) sr.flipX = false;
    }
}
