using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LockedDoor : MonoBehaviour
{
    private bool _playerNear;
    private PlayerInventory _playerInv;

    private void Awake()
    {
        // Require a trigger collider to detect the player
        var trigger = gameObject.AddComponent<CircleCollider2D>();
        trigger.radius = 1.5f;
        trigger.isTrigger = true;
    }

    private void Update()
    {
        if (_playerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (_playerInv != null && _playerInv.HasKey)
            {
                // Unlock!
                AudioManager.Instance?.PlayPickup(); // TODO: add unlock sound later
                Destroy(gameObject);
            }
            else
            {
                // Optional: play a "locked" sound or show UI
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerNear = true;
            _playerInv = other.GetComponent<PlayerInventory>();
            
            // Show interaction tip if desired
            if (_playerInv != null && UIManager.Instance != null && UIManager.Instance.sneakTooltip != null)
            {
                var txt = UIManager.Instance.sneakTooltip.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = _playerInv.HasKey ? "Press E to Unlock Door" : "Locked (Find the Key)";
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerNear = false;
            _playerInv = null;
            
            if (UIManager.Instance != null && UIManager.Instance.sneakTooltip != null)
            {
                var txt = UIManager.Instance.sneakTooltip.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (txt != null) txt.text = "Hold SHIFT to sneak  |  E to pick up";
            }
        }
    }
}
