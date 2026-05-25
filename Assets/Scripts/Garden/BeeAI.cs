using UnityEngine;

/// <summary>
/// Simple AI for a Bee/Butterfly that spawns near Flowers.
/// Has a proper procedural bee sprite with body, stripes and wings.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BeeAI : MonoBehaviour
{
    private SpriteRenderer _sr;
    private Vector3 _startPos;
    private float _timeOffset;
    private float _flySpeed = 1.2f;
    private float _wanderRadius = 1.2f;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr.sprite == null)
        {
            _sr.sprite = GenerateBeeSprite();
        }
        _sr.sortingOrder = 30;
        transform.localScale = new Vector3(0.6f, 0.6f, 1f);
    }

    private void Start()
    {
        _startPos = transform.position;
        _timeOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        float t = Time.time + _timeOffset;
        float x = Mathf.Sin(t * _flySpeed) * _wanderRadius;
        float y = Mathf.Cos(t * _flySpeed * 1.7f) * (_wanderRadius * 0.5f);

        Vector3 newPos = _startPos + new Vector3(x, y, 0);

        if (newPos.x > transform.position.x) _sr.flipX = false;
        else if (newPos.x < transform.position.x) _sr.flipX = true;

        transform.position = newPos;
    }

    private static Sprite GenerateBeeSprite()
    {
        int size = 16;
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        Color clear = Color.clear;
        Color yellow = new Color(1f, 0.85f, 0.1f);
        Color black = new Color(0.15f, 0.1f, 0.05f);
        Color wing = new Color(0.85f, 0.95f, 1f, 0.7f);

        // Clear
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        // Body (oval, rows 4-11, cols 4-11)
        for (int y = 4; y <= 11; y++)
        {
            for (int x = 4; x <= 11; x++)
            {
                float dx = (x - 7.5f) / 4f;
                float dy = (y - 7.5f) / 4f;
                if (dx * dx + dy * dy <= 1f)
                {
                    // Stripes
                    bool stripe = ((y - 4) / 2) % 2 == 0;
                    tex.SetPixel(x, y, stripe ? yellow : black);
                }
            }
        }

        // Eyes
        tex.SetPixel(10, 9, Color.white);
        tex.SetPixel(10, 8, Color.white);
        tex.SetPixel(11, 9, black);
        tex.SetPixel(11, 8, black);

        // Wings (top)
        for (int y = 11; y <= 14; y++)
        {
            for (int x = 5; x <= 10; x++)
            {
                float dx = (x - 7.5f) / 3f;
                float dy = (y - 12.5f) / 2f;
                if (dx * dx + dy * dy <= 1f)
                    tex.SetPixel(x, y, wing);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }
}
