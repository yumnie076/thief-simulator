using UnityEngine;

/// <summary>
/// A small snack (worm/berry) that spawns in the garden and can be eaten by the hedgehog.
/// Has a proper procedural berry sprite with shine and a small bob animation.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Collectible : MonoBehaviour
{
    private float _bobOffset;
    private Vector3 _basePos;

    private void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        if (sr.sprite == null)
        {
            sr.sprite = GenerateBerrySprite();
        }

        sr.sortingOrder = 40;
        transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        _bobOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Start()
    {
        _basePos = transform.position;
    }

    private void Update()
    {
        // Small gentle bob up and down
        float bob = Mathf.Sin(Time.time * 2f + _bobOffset) * 0.06f;
        transform.position = _basePos + new Vector3(0, bob, 0);
    }

    private static Sprite GenerateBerrySprite()
    {
        int size = 12;
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        Color clear = Color.clear;
        Color berry = new Color(0.85f, 0.15f, 0.25f); // Red berry
        Color dark = new Color(0.55f, 0.08f, 0.15f);
        Color shine = new Color(1f, 0.6f, 0.65f);
        Color stem = new Color(0.3f, 0.55f, 0.15f);

        // Clear
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        // Berry body (circle)
        for (int y = 1; y <= 8; y++)
        {
            for (int x = 2; x <= 9; x++)
            {
                float dx = (x - 5.5f) / 4f;
                float dy = (y - 4.5f) / 4f;
                if (dx * dx + dy * dy <= 1f)
                {
                    // Shading: top-left is lighter
                    if (dx < -0.3f && dy > 0.3f)
                        tex.SetPixel(x, y, shine);
                    else if (dx > 0.5f || dy < -0.5f)
                        tex.SetPixel(x, y, dark);
                    else
                        tex.SetPixel(x, y, berry);
                }
            }
        }

        // Stem on top
        tex.SetPixel(5, 9, stem);
        tex.SetPixel(6, 9, stem);
        tex.SetPixel(5, 10, stem);
        tex.SetPixel(7, 10, stem); // small leaf

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }
}
