using UnityEngine;

/// <summary>
/// Top-level guard brain. Owns state machine transitions.
/// Patrol and Vision logic live in separate components.
/// </summary>
public class GuardController : MonoBehaviour
{
    public enum GuardState { Patrol, Investigate, Caught }
    public GuardState State { get; private set; } = GuardState.Patrol;

    [Header("Investigate")]
    public float investigateRotateTime = 2f;  // 360° scan duration

    private GuardPatrol _patrol;
    private GuardVision _vision;
    private Rigidbody2D _rb;

    private Vector2 _investigateTarget;
    private bool    _isInvestigating;
    private float   _investigateTimer;
    private bool    _isRotating;
    private float   _rotateTimer;

    private void Awake()
    {
        _patrol = GetComponent<GuardPatrol>();
        _vision = GetComponent<GuardVision>();
        _rb     = GetComponent<Rigidbody2D>();
        if (_rb)
        {
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }
    }

    private void Update()
    {
        if (GameManager.Instance?.State != GameManager.GameState.Playing) return;

        switch (State)
        {
            case GuardState.Patrol:     UpdatePatrol();     break;
            case GuardState.Investigate: UpdateInvestigate(); break;
        }
    }

    // ── Patrol ────────────────────────────────────────────────────────────────
    private void UpdatePatrol()
    {
        _patrol?.Tick();
    }

    // ── Investigate ───────────────────────────────────────────────────────────
    public void OnHearPlayer(Vector2 soundOrigin)
    {
        if (State == GuardState.Caught) return;
        if (State == GuardState.Patrol)
        {
            _patrol?.Pause();
            State = GuardState.Investigate;
            _investigateTarget = soundOrigin;
            _isInvestigating = true;
            _isRotating = false;
            AudioManager.Instance?.PlayHuh();
        }
        else
        {
            // Update target while already investigating
            _investigateTarget = soundOrigin;
        }
    }

    private void UpdateInvestigate()
    {
        if (_isInvestigating)
        {
            // Walk to last heard position
            Vector2 pos  = transform.position;
            Vector2 dir  = (_investigateTarget - pos);
            float   dist = dir.magnitude;

            if (dist > 0.15f)
            {
                Vector2 move = dir.normalized * _patrol.speed * Time.deltaTime;
                transform.position = pos + move;
                FaceDirection(dir.normalized);
            }
            else
            {
                // Arrived — start 360° scan
                _isInvestigating = false;
                _isRotating = true;
                _rotateTimer = investigateRotateTime;
            }
        }
        else if (_isRotating)
        {
            _rotateTimer -= Time.deltaTime;
            float rotPerSec = 360f / investigateRotateTime;
            transform.Rotate(0f, 0f, rotPerSec * Time.deltaTime);

            if (_rotateTimer <= 0f)
            {
                // Resume patrol from nearest waypoint
                _isRotating = false;
                _patrol?.ResumeFromNearest(transform.position);
                State = GuardState.Patrol;
            }
        }
    }

    // ── Caught ────────────────────────────────────────────────────────────────
    public void OnPlayerCaught()
    {
        if (State == GuardState.Caught) return;
        State = GuardState.Caught;
        _patrol?.Pause();
        GameManager.Instance?.TriggerLose();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private void FaceDirection(Vector2 dir)
    {
        if (dir == Vector2.zero) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
