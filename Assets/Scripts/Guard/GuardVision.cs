using UnityEngine;

/// <summary>
/// Draws and tests the guard's vision cone.
/// Vision catch: player visible for > 0.3s continuous → caught.
/// </summary>
public class GuardVision : MonoBehaviour
{
    [Header("Vision")]
    public float baseRange      = 4f;
    public float halfAngle      = 30f;   // Half of 60° cone
    public LayerMask wallsMask;          // Set to "Walls" layer in Inspector

    [Header("Visual")]
    public int   coneSegments    = 20;
    public Color coneColor       = new Color(1f, 0.33f, 0.33f, 0.25f); // #ff5555 @ 25%

    private float   _visibleTimer;
    private const float CatchTime = 0.3f;

    private MeshFilter   _mf;
    private MeshRenderer _mr;
    private Mesh         _mesh;
    private GuardController _controller;
    private PlayerController _player;

    private void Awake()
    {
        _controller = GetComponent<GuardController>();

        // Build cone mesh renderer
        var go = new GameObject("VisionCone");
        go.transform.SetParent(transform, false);

        _mf   = go.AddComponent<MeshFilter>();
        _mr   = go.AddComponent<MeshRenderer>();
        _mesh = new Mesh();
        _mf.mesh = _mesh;

        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = Color.white; // Color is handled by vertex colors now
        _mr.material = mat;

        // Put cone behind sprites
        _mr.sortingLayerName = "Default";
        _mr.sortingOrder     = -1;
    }

    private void Start()
    {
        // Find player at runtime so we don't need a scene reference
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO) _player = playerGO.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (GameManager.Instance?.State != GameManager.GameState.Playing) return;
        if (_controller?.State == GuardController.GuardState.Caught) return;

        float visionPenalty = 1f;
        if (_player != null) visionPenalty = _player.guardVisionPenalty;
        float range = baseRange * visionPenalty;

        BuildConeMesh(range);
        CheckPlayerInCone(range);
    }

    private void CheckPlayerInCone(float range)
    {
        if (_player == null) return;

        Vector2 toPlayer = (Vector2)_player.transform.position - (Vector2)transform.position;
        float dist = toPlayer.magnitude;

        if (dist > range)
        {
            _visibleTimer = 0f;
            return;
        }

        float angle = Vector2.Angle(transform.up, toPlayer);
        if (angle > halfAngle)
        {
            _visibleTimer = 0f;
            return;
        }

        // Raycast to check wall occlusion
        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized,
                                              dist, wallsMask);
        if (hit.collider != null)
        {
            _visibleTimer = 0f;
            return;
        }

        // Player is visible
        _visibleTimer += Time.deltaTime;
        if (_visibleTimer >= CatchTime)
            _controller?.OnPlayerCaught();
    }

    private void BuildConeMesh(float range)
    {
        Vector3[] verts  = new Vector3[coneSegments + 2];
        Color[]   colors = new Color[coneSegments + 2];
        int[]     tris   = new int[coneSegments * 3];

        verts[0] = Vector3.zero; // cone origin (local)
        colors[0] = new Color(coneColor.r, coneColor.g, coneColor.b, 0.4f); // Center opacity

        float stepAngle = (halfAngle * 2f) / coneSegments;
        float startAngle = -halfAngle;

        for (int i = 0; i <= coneSegments; i++)
        {
            float a   = startAngle + i * stepAngle;
            float rad = a * Mathf.Deg2Rad;
            verts[i + 1] = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0f) * range;
            colors[i + 1] = new Color(coneColor.r, coneColor.g, coneColor.b, 0f); // Edge opacity
        }

        for (int i = 0; i < coneSegments; i++)
        {
            tris[i * 3]     = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = i + 2;
        }

        _mesh.Clear();
        _mesh.vertices  = verts;
        _mesh.colors    = colors;
        _mesh.triangles = tris;
        _mesh.RecalculateNormals();
    }
}
