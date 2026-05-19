using UnityEngine;

/// <summary>
/// Front-door exit. Player wins when overlapping this with >= 1 item.
/// Also pulses the sprite scale as a green glow indicator.
/// </summary>
public class ExitTrigger : MonoBehaviour
{
    public float pulseSpeed  = 1.5f;
    public float pulseAmount = 0.12f;
    private Vector3 _baseScale;

    private void Awake() => _baseScale = transform.localScale;

    private void Update()
    {
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = _baseScale * pulse;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var inv = other.GetComponent<PlayerInventory>();
        if (inv != null && inv.Count >= 1)
            GameManager.Instance?.TriggerWin();
    }
}
