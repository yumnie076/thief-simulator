using UnityEngine;

/// <summary>
/// Simple AI for a Frog that spawns near a Pond.
/// Has a proper procedural frog sprite and hop animation.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class FrogAI : MonoBehaviour
{
    private SpriteRenderer _sr;
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private float _waitTime;
    private float _hopSpeed = 2.5f;
    private float _wanderRadius = 1.8f;
    private bool _isHopping;
    private float _hopTimer;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr.sprite == null)
        {
            _sr.sprite = GenerateFrogSprite();
        }
        _sr.sortingOrder = 20;
        transform.localScale = new Vector3(0.7f, 0.7f, 1f);
    }

    private void Start()
    {
        _startPos = transform.position;
        PickNewTarget();
    }

    private void Update()
    {
        if (_waitTime > 0)
        {
            _waitTime -= Time.deltaTime;
            return;
        }

        // Hop animation: small Y bounce
        _hopTimer += Time.deltaTime * 8f;
        float hopY = Mathf.Abs(Mathf.Sin(_hopTimer)) * 0.15f;

        Vector3 flatPos = Vector3.MoveTowards(
            new Vector3(transform.position.x, transform.position.y - hopY, 0),
            _targetPos, _hopSpeed * Time.deltaTime);
        
        transform.position = new Vector3(flatPos.x, flatPos.y + hopY, 0);

        if (_targetPos.x > transform.position.x) _sr.flipX = false;
        else if (_targetPos.x < transform.position.x) _sr.flipX = true;

        if (Vector3.Distance(new Vector3(transform.position.x, transform.position.y, 0), _targetPos) < 0.15f)
        {
            _hopTimer = 0;
            PickNewTarget();
        }
    }

    private void PickNewTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * _wanderRadius;
        _targetPos = _startPos + new Vector3(randomCircle.x, randomCircle.y, 0);
        _waitTime = Random.Range(1.5f, 4f);
    }

    private static Sprite GenerateFrogSprite()
    {
        int size = 16;
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        Color clear = Color.clear;
        Color darkGreen = new Color(0.15f, 0.5f, 0.15f);
        Color lightGreen = new Color(0.3f, 0.75f, 0.25f);
        Color belly = new Color(0.65f, 0.85f, 0.45f);
        Color eye = Color.white;
        Color pupil = Color.black;

        // Clear
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        // Body (bottom oval, rows 2-9, cols 3-12)
        for (int y = 2; y <= 9; y++)
        {
            for (int x = 3; x <= 12; x++)
            {
                float dx = (x - 7.5f) / 5f;
                float dy = (y - 5.5f) / 4f;
                if (dx * dx + dy * dy <= 1f)
                {
                    tex.SetPixel(x, y, y <= 5 ? belly : lightGreen);
                }
            }
        }

        // Darker outline on top
        for (int x = 5; x <= 10; x++)
            tex.SetPixel(x, 9, darkGreen);

        // Eyes (two bumps on top)
        // Left eye
        tex.SetPixel(4, 10, lightGreen);
        tex.SetPixel(5, 10, lightGreen);
        tex.SetPixel(4, 11, lightGreen);
        tex.SetPixel(5, 11, lightGreen);
        tex.SetPixel(5, 11, eye);
        tex.SetPixel(5, 10, pupil);

        // Right eye
        tex.SetPixel(10, 10, lightGreen);
        tex.SetPixel(11, 10, lightGreen);
        tex.SetPixel(10, 11, lightGreen);
        tex.SetPixel(11, 11, lightGreen);
        tex.SetPixel(10, 11, eye);
        tex.SetPixel(10, 10, pupil);

        // Legs (small L-shapes)
        // Front legs
        tex.SetPixel(3, 4, darkGreen);
        tex.SetPixel(2, 4, darkGreen);
        tex.SetPixel(2, 3, darkGreen);
        tex.SetPixel(12, 4, darkGreen);
        tex.SetPixel(13, 4, darkGreen);
        tex.SetPixel(13, 3, darkGreen);

        // Back legs
        tex.SetPixel(3, 7, darkGreen);
        tex.SetPixel(2, 7, darkGreen);
        tex.SetPixel(1, 7, darkGreen);
        tex.SetPixel(1, 6, darkGreen);
        tex.SetPixel(12, 7, darkGreen);
        tex.SetPixel(13, 7, darkGreen);
        tex.SetPixel(14, 7, darkGreen);
        tex.SetPixel(14, 6, darkGreen);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }
}
