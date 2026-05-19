using UnityEngine;

/// <summary>
/// Placed on each item prefab in the scene.
/// Handles proximity detection (trigger) and hands off to PlayerInventory.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public Item itemData;

    [Header("Glow Animation")]
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.08f;

    private Vector3 _baseScale;
    private SpriteRenderer _sr;
    private float _glowTime;

    private bool _isPickedUp = false;

    private void Awake()
    {
        _baseScale = transform.localScale;
        _sr = GetComponent<SpriteRenderer>();

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (_isPickedUp) return;
        _glowTime += Time.deltaTime * pulseSpeed;
        float pulse = 1f + Mathf.Sin(_glowTime) * pulseAmount;
        transform.localScale = _baseScale * pulse;
    }

    public void OnPickedUp()
    {
        if (_isPickedUp) return;
        _isPickedUp = true;
        GetComponent<Collider2D>().enabled = false;
        
        SpawnPopup();
        StartCoroutine(PickupRoutine());
    }

    private void SpawnPopup()
    {
        var go = new GameObject("Popup");
        go.transform.position = transform.position;
        var tmp = go.AddComponent<TMPro.TextMeshPro>();
        tmp.text = $"+{itemData.value}!";
        tmp.fontSize = 6;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = GetRarityColor(itemData.itemName);
        
        var mr = tmp.GetComponent<MeshRenderer>();
        mr.sortingLayerName = "Default";
        mr.sortingOrder = 20;

        StartCoroutine(PopupRoutine(go));
    }

    private Color GetRarityColor(string name)
    {
        switch(name)
        {
            case "Coin": return new Color32(184, 115, 51, 255);
            case "Ring": return new Color32(192, 192, 192, 255);
            case "Vase": return new Color32(160, 196, 216, 255);
            case "Necklace": return new Color32(205, 127, 50, 255);
            case "Painting": return new Color32(156, 124, 58, 255);
            case "Statue": return new Color32(232, 232, 224, 255);
            case "Crown": return new Color32(255, 215, 0, 255);
            default: return Color.white;
        }
    }

    private System.Collections.IEnumerator PickupRoutine()
    {
        // Scale punch up
        float t = 0;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            transform.localScale = _baseScale * Mathf.Lerp(1f, 1.4f, t / 0.1f);
            yield return null;
        }
        // Scale down to 0
        t = 0;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            transform.localScale = _baseScale * Mathf.Lerp(1.4f, 0f, t / 0.15f);
            yield return null;
        }
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator PopupRoutine(GameObject go)
    {
        Transform t = go.transform;
        TMPro.TextMeshPro tmp = go.GetComponent<TMPro.TextMeshPro>();
        Vector3 startPos = t.position;
        
        // Punch scale
        float elapsed = 0;
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.one * Mathf.Lerp(0f, 1.3f, elapsed / 0.15f);
            yield return null;
        }
        elapsed = 0;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.one * Mathf.Lerp(1.3f, 1.0f, elapsed / 0.1f);
            yield return null;
        }

        // Drift up and fade
        elapsed = 0;
        while (elapsed < 0.7f)
        {
            elapsed += Time.deltaTime;
            t.position = startPos + Vector3.up * Mathf.Lerp(0, 1.5f, elapsed / 0.7f);
            Color c = tmp.color;
            c.a = Mathf.Lerp(1f, 0f, elapsed / 0.7f);
            tmp.color = c;
            yield return null;
        }
        Destroy(go);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isPickedUp) return;
        var inv = other.GetComponent<PlayerInventory>();
        if (inv != null) inv.RegisterNearby(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_isPickedUp) return;
        var inv = other.GetComponent<PlayerInventory>();
        if (inv != null) inv.UnregisterNearby(this);
    }
}
