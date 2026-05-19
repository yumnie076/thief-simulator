using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // ── Constants ────────────────────────────────────────────────────────────
    public float baseSpeed          = 5f;
    public float sneakSoundMultiplier = 0.5f;

    // ── Weight effects ───────────────────────────────────────────────────────
    //   Applied in RecalculateWeight() whenever player picks something up.
    [HideInInspector] public float speedMult           = 1f;
    [HideInInspector] public float soundMult           = 1f;
    [HideInInspector] public float guardVisionPenalty  = 1f;

    // ── Internal ─────────────────────────────────────────────────────────────
    private Rigidbody2D _rb;
    private PlayerInventory _inventory;
    private PlayerSound _sound;
    private SpriteRenderer _sr;

    public bool IsSneaking { get; private set; }
    private Vector2 _moveDir;

    // Facing sprites: assign in Inspector (Up/Down/Left/Right)
    [Header("Directional Sprites")]
    public Sprite spriteUp;
    public Sprite spriteDown;
    public Sprite spriteLeft;
    public Sprite spriteRight;

    [Header("Effects")]
    public GameObject footstepPrefab;
    private float _footstepTimer;

    private void Awake()
    {
        _rb        = GetComponent<Rigidbody2D>();
        _inventory = GetComponent<PlayerInventory>();
        _sound     = GetComponent<PlayerSound>();
        _sr        = GetComponent<SpriteRenderer>();

        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing) return;

        // Movement input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _moveDir = new Vector2(h, v).normalized;

        // Sneak
        IsSneaking = Input.GetKey(KeyCode.LeftShift);

        // Facing sprite
        UpdateFacingSprite();

        // Pickup
        if (Input.GetKeyDown(KeyCode.E))
            _inventory?.TryPickup();

        // Footsteps
        if (_moveDir != Vector2.zero && footstepPrefab != null)
        {
            float interval = IsSneaking ? 1f : 0.5f;
            _footstepTimer -= Time.deltaTime;
            if (_footstepTimer <= 0)
            {
                _footstepTimer = interval;
                Instantiate(footstepPrefab, transform.position, Quaternion.identity);
            }
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        float sneakFactor = IsSneaking ? 0.5f : 1f;
        float currentSpeed = baseSpeed * speedMult * sneakFactor;
        _rb.linearVelocity = _moveDir * currentSpeed;
    }

    private void UpdateFacingSprite()
    {
        if (_sr == null || _moveDir == Vector2.zero) return;

        if (Mathf.Abs(_moveDir.x) > Mathf.Abs(_moveDir.y))
        {
            if (_moveDir.x > 0 && spriteRight) _sr.sprite = spriteRight;
            else if (spriteLeft)               _sr.sprite = spriteLeft;
        }
        else
        {
            if (_moveDir.y > 0 && spriteUp) _sr.sprite = spriteUp;
            else if (spriteDown)            _sr.sprite = spriteDown;
        }
    }

    /// <summary>Called by PlayerInventory after a pickup.</summary>
    public void RecalculateWeight(int totalWeight)
    {
        speedMult          = 1f / (1f + totalWeight * 0.08f);
        soundMult          = 1f + (totalWeight * 0.15f);
        guardVisionPenalty = 1f + (totalWeight * 0.05f);
        _sound?.UpdateRadius();
    }
}
