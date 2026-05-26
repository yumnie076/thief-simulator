using UnityEngine;

/// <summary>
/// A friendly hedgehog that bobs gently in the garden.
/// When the player hedgehog touches it, it is "found" for bonus score.
/// </summary>
public class FriendHedgehog : MonoBehaviour
{
    public bool found = false;

    private Vector3 basePosition;
    private SpriteRenderer sr;
    private float flipTimer;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        basePosition = transform.position;
        flipTimer = Random.Range(2f, 5f);
    }

    private void Update()
    {
        if (found) return;

        // Gentle bobbing animation
        float bobY = Mathf.Sin(Time.time * 2f) * 0.1f;
        transform.position = new Vector3(basePosition.x, basePosition.y + bobY, basePosition.z);

        // Flip sprite occasionally
        flipTimer -= Time.deltaTime;
        if (flipTimer <= 0f)
        {
            if (sr != null) sr.flipX = !sr.flipX;
            flipTimer = Random.Range(2f, 5f);
        }
    }
}
