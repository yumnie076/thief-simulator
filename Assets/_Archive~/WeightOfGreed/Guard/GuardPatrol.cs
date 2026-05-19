using UnityEngine;

/// <summary>
/// Handles waypoint-based patrol movement.
/// Paused during Investigate state by GuardController.
/// </summary>
public class GuardPatrol : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Movement")]
    public float speed        = 3f;
    public float waitDuration = 0.5f;

    private int   _current;
    private bool  _waiting;
    private float _waitTimer;
    private bool  _paused;

    private GuardController _controller;

    private void Awake()
    {
        _controller = GetComponent<GuardController>();
    }

    public void Tick()
    {
        if (_paused || waypoints == null || waypoints.Length == 0) return;

        if (_waiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _waiting = false;
                AdvanceWaypoint();
            }
            return;
        }

        // Move toward current waypoint
        Vector2 pos    = transform.position;
        Vector2 target = waypoints[_current].position;
        Vector2 dir    = (target - pos);
        float   dist   = dir.magnitude;

        if (dist < 0.1f)
        {
            // Arrived
            transform.position = target;
            _waiting   = true;
            _waitTimer = waitDuration;
        }
        else
        {
            transform.position = pos + dir.normalized * speed * Time.deltaTime;
            FaceDirection(dir.normalized);
        }
    }

    private void AdvanceWaypoint()
    {
        _current = (_current + 1) % waypoints.Length;
    }

    public void Pause()  => _paused = true;

    public void ResumeFromNearest(Vector2 position)
    {
        _paused = false;
        _waiting = false;

        // Find nearest waypoint to jump back to patrol cleanly
        float minDist = float.MaxValue;
        int nearest = 0;
        for (int i = 0; i < waypoints.Length; i++)
        {
            float d = Vector2.Distance(position, waypoints[i].position);
            if (d < minDist) { minDist = d; nearest = i; }
        }
        _current = nearest;
    }

    private void FaceDirection(Vector2 dir)
    {
        if (dir == Vector2.zero) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
