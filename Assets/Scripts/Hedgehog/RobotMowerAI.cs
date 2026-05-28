using UnityEngine;

/// <summary>
/// The deadly Robot Mower. Moves in straight lines and bounces off walls and solid objects.
/// Instantly ends the game if it touches the hedgehog.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class RobotMowerAI : MonoBehaviour
{
    public float speed = 4.5f;
    private Vector2 direction;

    private void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;

        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true; // Kill zone trigger
        col.size = new Vector2(0.8f, 0.8f);

        var sr = GetComponent<SpriteRenderer>();
        sr.sortingOrder = 45;

        // Generate a visual representation (detailed top-down robot mower)
        var tex = new Texture2D(16, 16);
        tex.filterMode = FilterMode.Point;
        Color clear = Color.clear;
        Color body = new Color(0.2f, 0.2f, 0.2f);
        Color bodyHighlight = new Color(0.35f, 0.35f, 0.35f);
        Color wheel = new Color(0.05f, 0.05f, 0.05f);
        Color bumper = new Color(0.9f, 0.6f, 0.1f); // Orange warning
        Color light = new Color(0.9f, 0.1f, 0.1f); // Red danger light
        Color blade = new Color(0.7f, 0.7f, 0.7f); // Silver blades

        for(int y=0; y<16; y++)
        {
            for(int x=0; x<16; x++) 
            {
                tex.SetPixel(x, y, clear);

                // Body (x: 3 to 12, y: 2 to 13)
                if (x >= 3 && x <= 12 && y >= 2 && y <= 13)
                {
                    // Round the corners
                    if ((x==3 && y==2) || (x==12 && y==2) || (x==3 && y==13) || (x==12 && y==13)) continue;
                    
                    tex.SetPixel(x, y, body);
                    // Add some shine
                    if (x >= 5 && x <= 10 && y >= 5 && y <= 10) tex.SetPixel(x, y, bodyHighlight);
                }
                
                // Wheels (left and right)
                if ((x == 1 || x == 2 || x == 13 || x == 14) && (y >= 4 && y <= 10)) tex.SetPixel(x, y, wheel);

                // Front bumper (y = 13 and 14)
                if (x >= 4 && x <= 11 && (y == 13 || y == 14)) 
                {
                    // Striped yellow/orange warning pattern
                    if ((x + y) % 2 == 0) tex.SetPixel(x, y, bumper);
                    else tex.SetPixel(x, y, Color.yellow);
                }

                // Blades (y = 15, sticking out front)
                if ((x == 5 || x == 7 || x == 8 || x == 10) && y == 15) tex.SetPixel(x, y, blade);

                // Red light on top (center)
                if (x >= 7 && x <= 8 && y >= 6 && y <= 7) tex.SetPixel(x, y, light);
            }
        }
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0,0,16,16), new Vector2(0.5f,0.5f), 16f);

        PickRandomOrthogonalDirection();
    }

    private void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // Rotate visually to match direction
        if (direction == Vector2.up) transform.rotation = Quaternion.Euler(0,0,0);
        else if (direction == Vector2.left) transform.rotation = Quaternion.Euler(0,0,90);
        else if (direction == Vector2.down) transform.rotation = Quaternion.Euler(0,0,180);
        else if (direction == Vector2.right) transform.rotation = Quaternion.Euler(0,0,270);

        // Bounce if hitting garden bounds
        if (GardenManager.Instance != null)
        {
            float gw = GardenManager.Instance.GardenWidth;
            float gh = GardenManager.Instance.GardenHeight;
            bool bounced = false;

            if (transform.position.x < 1f) { transform.position = new Vector3(1f, transform.position.y, 0f); bounced = true; }
            if (transform.position.x > gw - 1f) { transform.position = new Vector3(gw - 1f, transform.position.y, 0f); bounced = true; }
            if (transform.position.y < 1f) { transform.position = new Vector3(transform.position.x, 1f, 0f); bounced = true; }
            if (transform.position.y > gh - 1f) { transform.position = new Vector3(transform.position.x, gh - 1f, 0f); bounced = true; }
            
            if (bounced) PickRandomOrthogonalDirection();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Bounce off solid objects (trees, houses) but NOT the player/triggers
        var go = other.GetComponent<GardenObject>();
        if (go != null && !other.isTrigger)
        {
            // Simple bounce: reverse direction and pick a new one
            transform.position -= (Vector3)direction * 0.1f; // Back up slightly
            PickRandomOrthogonalDirection();
        }
    }

    private void PickRandomOrthogonalDirection()
    {
        int r = Random.Range(0, 4);
        if (r == 0) direction = Vector2.up;
        else if (r == 1) direction = Vector2.down;
        else if (r == 2) direction = Vector2.left;
        else direction = Vector2.right;
    }
}
