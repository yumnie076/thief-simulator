using UnityEngine;

/// <summary>
/// Handles top-down movement for the Player Character.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private SpriteRenderer sr;

    [Header("Interaction")]
    public float interactDistance = 1.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // Ensure rigidbody is set up correctly for top down movement
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        // Get input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Normalize to prevent faster diagonal movement
        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        // Flip sprite based on movement direction
        if (movement.x < 0)
            sr.flipX = true;
        else if (movement.x > 0)
            sr.flipX = false;
    }

    private void FixedUpdate()
    {
        // Apply movement
        Vector2 targetPos = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

        // Clamp to garden boundaries
        if (GardenManager.Instance != null)
        {
            float w = GardenManager.Instance.GardenWidth;
            float h = GardenManager.Instance.GardenHeight;
            targetPos.x = Mathf.Clamp(targetPos.x, 0.5f, w - 0.5f);
            targetPos.y = Mathf.Clamp(targetPos.y, 0.5f, h - 0.5f);
        }

        rb.MovePosition(targetPos);
    }

    private void LateUpdate()
    {
        if (sr != null)
        {
            sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10f);
        }
    }
}
