using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Free-roaming Hedgehog AI for the ecosystem overhaul.
/// Wanders around, hunts bugs, and sleeps.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
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
    public float moveSpeed = 2.5f;

    private SpriteRenderer sr;
    private Insect targetInsect;
    private Vector3 targetPos;
    private float waitTime;
    
    public int bugsEaten = 0;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        var col = GetComponent<BoxCollider2D>();
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);
    }

    private void Start()
    {
        PickRandomWanderTarget();
    }

    private void Update()
    {
        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
            return;
        }

        switch (currentState)
        {
            case HedgehogState.Wander:
                DoWander();
                break;
            case HedgehogState.Hunt:
                DoHunt();
                break;
            case HedgehogState.Eat:
                // Just waiting...
                break;
            case HedgehogState.Sleep:
                // Sleeping...
                break;
        }

        // Depth sorting
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10f);
    }

    private void DoWander()
    {
        // Check for nearby insects occasionally
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

        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            // Eat the bug
            EcosystemManager.Instance.RemoveInsect(targetInsect);
            targetInsect = null;
            bugsEaten++;
            
            // Check quest progression if we add it later...
            
            // Go to eat state
            currentState = HedgehogState.Eat;
            waitTime = 2f;
            StartCoroutine(FinishEating());
        }
    }

    private IEnumerator FinishEating()
    {
        yield return new WaitForSeconds(2f);
        
        // Go sleep if eaten enough
        if (bugsEaten >= 3)
        {
            FindHouse();
            if (targetPos != Vector3.zero)
            {
                currentState = HedgehogState.Sleep;
                yield break;
            }
        }
        
        currentState = HedgehogState.Wander;
        PickRandomWanderTarget();
    }

    private void FindHouse()
    {
        foreach (var obj in GardenManager.Instance.PlacedObjects)
        {
            if (obj.Type == GardenObject.ObjectType.HedgehogHouse)
            {
                targetPos = obj.transform.position;
                return;
            }
        }
        targetPos = Vector3.zero; // No house found
    }

    private void FindNearestInsect()
    {
        float bestDist = 8f; // Hunt radius
        targetInsect = null;

        if (EcosystemManager.Instance == null) return;

        foreach (var insect in EcosystemManager.Instance.AllInsects)
        {
            if (insect.type == Insect.InsectType.Butterfly) continue; // Doesn't eat butterflies

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
