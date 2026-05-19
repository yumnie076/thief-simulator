using UnityEngine;

/// <summary>
/// Visualizes the player's sound emission radius as a semi-transparent circle.
/// Attach to a child GameObject of the Player with a CircleCollider2D (trigger)
/// for guard hearing detection.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class SoundRadius : MonoBehaviour
{
    [Header("Visual")]
    public int   circleSegments = 64;
    public Color circleColor    = new Color(1f, 0.96f, 0.43f, 0.20f); // #fff66e @ 20%

    private SpriteRenderer _sr;
    private CircleCollider2D _col;
    private float          _currentRadius = 1f;

    private void Awake()
    {
        _col = GetComponent<CircleCollider2D>();
        _col.isTrigger = true;

        _sr = gameObject.AddComponent<SpriteRenderer>();
        _sr.sortingLayerName = "Default";
        _sr.sortingOrder = 5;
        _sr.material = new Material(Shader.Find("Sprites/Default"));

        // Generate radial gradient
        int s = 128;
        float r = s / 2f;
        var tex = new Texture2D(s, s) { filterMode = FilterMode.Bilinear };
        var px = new Color[s * s];
        var ctr = new Vector2(r, r);
        for (int i = 0; i < px.Length; i++)
        {
            float d = Vector2.Distance(new Vector2(i % s, i / s), ctr) / r;
            px[i] = circleColor;
            px[i].a = d > 1f ? 0f : Mathf.Lerp(circleColor.a, 0f, d * d);
        }
        tex.SetPixels(px); tex.Apply();
        _sr.sprite = Sprite.Create(tex, new Rect(0,0,s,s), new Vector2(0.5f,0.5f), r); // Sprite is exactly 2 units wide (radius 1)
    }

    public void SetRadius(float radius)
    {
        if (Mathf.Approximately(_currentRadius, radius)) return;
        _currentRadius = radius;
        _col.radius = radius;
        transform.localScale = new Vector3(radius, radius, 1f);
    }

    // Guard hearing — called from GuardVision
    private void OnTriggerStay2D(Collider2D other)
    {
        var guard = other.GetComponentInParent<GuardController>();
        guard?.OnHearPlayer(transform.root.position);
    }
}
