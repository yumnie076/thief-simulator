using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool HasKey { get; set; } = false;

    // ── Nearby items tracked via OnTrigger ───────────────────────────────────
    private readonly List<ItemPickup> _nearby = new List<ItemPickup>();
    private PlayerController _controller;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    // Called by items entering the player's collider radius
    public void RegisterNearby(ItemPickup item)
    {
        if (!_nearby.Contains(item)) _nearby.Add(item);
    }

    public void UnregisterNearby(ItemPickup item)
    {
        _nearby.Remove(item);
    }

    public void TryPickup()
    {
        if (_nearby.Count == 0) return;

        // Pick the closest item
        ItemPickup closest = null;
        float minDist = float.MaxValue;
        foreach (var item in _nearby)
        {
            if (item == null) continue;
            float d = Vector2.Distance(transform.position, item.transform.position);
            if (d < minDist) { minDist = d; closest = item; }
        }

        if (closest == null) return;

        // Add to score/weight
        ScoreManager.Instance?.AddItem(closest.itemData.value, closest.itemData.weight);
        AudioManager.Instance?.PlayPickup();
        UIManager.Instance?.FlashPickup();

        // Recalculate player stats
        _controller?.RecalculateWeight(ScoreManager.Instance?.TotalWeight ?? 0);

        // Remove item from world with juice
        _nearby.Remove(closest);
        closest.OnPickedUp();
    }

    public int Count => ScoreManager.Instance?.ItemCount ?? 0;
}
