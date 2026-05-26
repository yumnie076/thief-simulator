using UnityEngine;
using System.Collections;

/// <summary>
/// Lightweight procedural particle system for 2D.
/// Spawns small GameObjects with SpriteRenderers that animate and self-destruct.
/// Does NOT use Unity's ParticleSystem component.
/// </summary>
public static class SimpleParticle
{
    // ─── Cached sprites (created once, reused forever) ───
    private static Sprite _heartSprite;
    private static Sprite _leafSprite;
    private static Sprite _sparkleSprite;
    private static Sprite _dropletSprite;

    private const int SortingOrder = 200;
    private const float DefaultDuration = 0.8f;

    // ─── Public API ───

    /// <summary>
    /// Spawns small red hearts that float upward and fade. Used when hedgehog eats.
    /// </summary>
    public static void SpawnHearts(Vector3 position, int count = 3)
    {
        EnsureHeartSprite();
        for (int i = 0; i < count; i++)
        {
            float xOff = Random.Range(-0.3f, 0.3f);
            float ySpeed = Random.Range(0.8f, 1.4f);
            Vector3 start = position + new Vector3(xOff, 0f, 0f);
            SpawnParticle(_heartSprite, start, (t) =>
            {
                // Float upward
                return new Vector3(0f, ySpeed * t, 0f);
            });
        }
    }

    /// <summary>
    /// Spawns small green leaves that drift down and sideways. Used when placing trees/bushes.
    /// </summary>
    public static void SpawnLeaves(Vector3 position, int count = 5)
    {
        EnsureLeafSprite();
        for (int i = 0; i < count; i++)
        {
            float xDir = Random.Range(-1f, 1f);
            float ySpeed = Random.Range(0.4f, 0.9f);
            float xOff = Random.Range(-0.5f, 0.5f);
            float yOff = Random.Range(0f, 0.3f);
            Vector3 start = position + new Vector3(xOff, yOff, 0f);
            SpawnParticle(_leafSprite, start, (t) =>
            {
                // Drift down and sideways with slight sine wobble
                float x = xDir * t + Mathf.Sin(t * 6f) * 0.1f;
                float y = -ySpeed * t;
                return new Vector3(x, y, 0f);
            });
        }
    }

    /// <summary>
    /// Spawns small yellow sparkles that pop outward and fade. Used when placing flowers.
    /// </summary>
    public static void SpawnSparkles(Vector3 position, int count = 4)
    {
        EnsureSparkleSprite();
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float speed = Random.Range(0.6f, 1.2f);
            float dirX = Mathf.Cos(angle) * speed;
            float dirY = Mathf.Sin(angle) * speed;
            Vector3 start = position;
            SpawnParticle(_sparkleSprite, start, (t) =>
            {
                // Pop outward from center
                return new Vector3(dirX * t, dirY * t, 0f);
            });
        }
    }

    /// <summary>
    /// Spawns blue water droplets that arc upward then fall. Used when placing ponds.
    /// </summary>
    public static void SpawnSplash(Vector3 position, int count = 4)
    {
        EnsureDropletSprite();
        for (int i = 0; i < count; i++)
        {
            float xDir = Random.Range(-1f, 1f);
            float upSpeed = Random.Range(1.2f, 2f);
            float gravity = 3f;
            Vector3 start = position;
            SpawnParticle(_dropletSprite, start, (t) =>
            {
                // Arc upward then fall (parabola)
                float x = xDir * t;
                float y = upSpeed * t - 0.5f * gravity * t * t;
                return new Vector3(x, y, 0f);
            });
        }
    }

    // ─── Core spawner ───

    private delegate Vector3 MotionFunc(float time);

    private static void SpawnParticle(Sprite sprite, Vector3 startPos, MotionFunc motion)
    {
        var go = new GameObject("Particle");
        go.transform.position = startPos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = SortingOrder;

        var animator = go.AddComponent<ParticleAnimator>();
        animator.Init(sr, startPos, motion, DefaultDuration);
    }

    // ─── Texture / Sprite generation (lazy, cached) ───

    private static void EnsureHeartSprite()
    {
        if (_heartSprite != null) return;

        const int size = 12;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color red = new Color(0.9f, 0.2f, 0.3f);

        // Clear
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        // Heart shape (two circles + triangle)
        // Left bump
        FillCircle(tex, 4, 8, 3, red);
        // Right bump
        FillCircle(tex, 8, 8, 3, red);
        // Bottom triangle fill
        for (int y = 2; y < 8; y++)
        {
            int halfW = (y - 1);
            if (halfW > 5) halfW = 5;
            for (int x = 6 - halfW; x <= 6 + halfW; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                    tex.SetPixel(x, y, red);
            }
        }

        tex.Apply();
        _heartSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 12f);
        _heartSprite.name = "particle_heart";
    }

    private static void EnsureLeafSprite()
    {
        if (_leafSprite != null) return;

        const int size = 8;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color green = new Color(0.3f, 0.7f, 0.2f);
        Color vein = new Color(0.2f, 0.5f, 0.15f);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        // Leaf: filled ellipse
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float nx = (x - 3.5f) / 3.5f;
                float ny = (y - 3.5f) / 2.5f;
                if (nx * nx + ny * ny <= 1f)
                    tex.SetPixel(x, y, green);
            }
        // Center vein
        for (int x = 1; x < 7; x++)
            tex.SetPixel(x, 4, vein);

        tex.Apply();
        _leafSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 8f);
        _leafSprite.name = "particle_leaf";
    }

    private static void EnsureSparkleSprite()
    {
        if (_sparkleSprite != null) return;

        const int size = 8;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color yellow = new Color(1f, 0.9f, 0.3f);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        // Star/cross shape
        // Vertical bar
        for (int y = 0; y < size; y++)
        {
            tex.SetPixel(3, y, yellow);
            tex.SetPixel(4, y, yellow);
        }
        // Horizontal bar
        for (int x = 0; x < size; x++)
        {
            tex.SetPixel(x, 3, yellow);
            tex.SetPixel(x, 4, yellow);
        }
        // Diagonal accents (corners)
        tex.SetPixel(1, 1, yellow);
        tex.SetPixel(6, 1, yellow);
        tex.SetPixel(1, 6, yellow);
        tex.SetPixel(6, 6, yellow);

        tex.Apply();
        _sparkleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 8f);
        _sparkleSprite.name = "particle_sparkle";
    }

    private static void EnsureDropletSprite()
    {
        if (_dropletSprite != null) return;

        const int size = 8;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color blue = new Color(0.3f, 0.6f, 0.95f);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        // Water droplet: circle bottom + point top
        FillCircle(tex, 4, 3, 3, blue);
        // Taper top
        tex.SetPixel(3, 5, blue);
        tex.SetPixel(4, 5, blue);
        tex.SetPixel(4, 6, blue);
        tex.SetPixel(3, 6, blue);
        tex.SetPixel(4, 7, blue);

        tex.Apply();
        _dropletSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 8f);
        _dropletSprite.name = "particle_droplet";
    }

    // ─── Drawing helper ───

    private static void FillCircle(Texture2D tex, int cx, int cy, int r, Color c)
    {
        for (int y = -r; y <= r; y++)
            for (int x = -r; x <= r; x++)
                if (x * x + y * y <= r * r)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                        tex.SetPixel(px, py, c);
                }
    }

    // ═══════════════════════════════════════
    // Nested MonoBehaviour that runs the animation coroutine
    // ═══════════════════════════════════════

    private class ParticleAnimator : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private Vector3 _startPos;
        private MotionFunc _motion;
        private float _duration;
        private Coroutine _routine;

        public void Init(SpriteRenderer sr, Vector3 startPos, MotionFunc motion, float duration)
        {
            _sr = sr;
            _startPos = startPos;
            _motion = motion;
            _duration = duration;
            _routine = StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float elapsed = 0f;
            Color baseColor = _sr.color;

            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _duration; // 0..1 normalized
                float tClamped = Mathf.Clamp01(t);

                // Position
                Vector3 offset = _motion(elapsed);
                transform.position = _startPos + offset;

                // Fade out alpha
                float alpha = 1f - tClamped;
                _sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

                // Slight scale down
                float scale = Mathf.Lerp(1f, 0.3f, tClamped);
                transform.localScale = Vector3.one * scale;

                yield return null;
            }

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }
    }
}
