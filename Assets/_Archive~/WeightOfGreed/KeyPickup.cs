using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var inv = other.GetComponent<PlayerInventory>();
            if (inv != null)
            {
                inv.HasKey = true;
                AudioManager.Instance?.PlayPickup();
                Destroy(gameObject);
            }
        }
    }
}
